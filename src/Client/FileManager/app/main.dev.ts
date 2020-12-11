/* eslint global-require: off, no-console: off */

/**
 * This module executes inside of electron's main process. You can start
 * electron renderer process from here and communicate with the other processes
 * through IPC.
 *
 * When running `yarn build` or `yarn build-main`, this file is compiled to
 * `./app/main.prod.js` using webpack. This gives us some performance wins.
 */
import 'core-js/stable';
import 'regenerator-runtime/runtime';
import { app, BrowserWindow, ipcMain, globalShortcut, dialog } from 'electron';
import { autoUpdater } from 'electron-updater';
import windowStateKeeper from 'electron-window-state';
import log from 'electron-log';
import path from 'path';
import MenuBuilder from './menu';

export default class AppUpdater {
  constructor() {
    log.transports.file.level = 'info';
    autoUpdater.logger = log;
    autoUpdater.checkForUpdatesAndNotify();
  }
}

let mainWindow: BrowserWindow | null = null;
(global as any).splashWorkerWindow = null;

if (process.env.NODE_ENV === 'production') {
  const sourceMapSupport = require('source-map-support');
  sourceMapSupport.install();
}

let startupFilePath: any;
let portableMode: boolean;

process.argv.forEach((arg, count) => {
  if (arg.toLowerCase() === '-d' || arg.toLowerCase() === '--debug') {
  } else if (arg.toLowerCase() === '-p' || arg.toLowerCase() === '--portable') {
    app.setPath('userData', process.cwd() + '/tsprofile');
    portableMode = true;
  } else if (arg.indexOf('-psn') >= 0) {
    arg = '';
  } else if (arg === './app/main.dev.babel.js' || arg === '.' || count === 0) {
  } else if (arg.length > 2) {
    if (arg !== './app/main.dev.js' && arg !== './app/') {
      startupFilePath = arg;
    }
  }
  if (portableMode) {
    startupFilePath = undefined;
  }
});

if (
  process.env.NODE_ENV === 'development' ||
  process.env.DEBUG_PROD === 'true'
) {
  require('electron-debug')({ showDevTools: false, devToolsMode: 'right' });
  const p = path.join(__dirname, '..', 'app', 'node_modules');
  require('module').globalPaths.push(p);
}

app.commandLine.appendSwitch('--disable-http-cache');

const installExtensions = async () => {
  const installer = require('electron-devtools-installer');
  const forceDownload = !!process.env.UPGRADE_EXTENSIONS;
  const extensions = ['REACT_DEVELOPER_TOOLS', 'REDUX_DEVTOOLS'];

  return Promise.all(
    extensions.map((name) => installer.default(installer[name], forceDownload))
  ).catch(console.log);
};

app.commandLine.appendSwitch('autoplay-policy', 'no-user-gesture-required'); // Fix broken autoplay functionality in the av player

const createWindow = async () => {
  let workerDevMode = false;
  let mainHTML = `file://${__dirname}/app.html`;

  if (
    process.env.NODE_ENV === 'development' ||
    process.env.DEBUG_PROD === 'true'
  ) {
    await installExtensions();
  }

  if (process.env.NODE_ENV === 'development') {
    workerDevMode = true;
    mainHTML = `file://${__dirname}/appd.html`;
  }

  const mainWindowState = windowStateKeeper({
    defaultWidth: 1280,
    defaultHeight: 800,
  });

  function createSplashWorker() {
    (global as any).splashWorkerWindow = new BrowserWindow({
      show: workerDevMode,
      x: 0,
      y: 0,
      width: workerDevMode ? 800 : 1,
      height: workerDevMode ? 600 : 1,
      frame: false,
      webPreferences: {
        nodeIntegration: true,
      },
    });

    (global as any).splashWorkerWindow.loadURL(
      `file://${__dirname}/splash.html`
    );
  }

  createSplashWorker();

  let startupParameter = '';
  if (startupFilePath) {
    if (startupFilePath.startsWith('./') || startupFilePath.startsWith('.\\')) {
      startupParameter =
        '?open=' + encodeURIComponent(path.join(__dirname, startupFilePath));
    } else {
      startupParameter = '?open=' + encodeURIComponent(startupFilePath);
    }
  }

  const RESOURCES_PATH = app.isPackaged
    ? path.join(process.resourcesPath, 'resources')
    : path.join(__dirname, '../resources');

  const getAssetPath = (...paths: string[]): string => {
    return path.join(RESOURCES_PATH, ...paths);
  };

  mainWindow = new BrowserWindow({
    show: true,
    x: mainWindowState.x,
    y: mainWindowState.y,
    width: mainWindowState.width,
    height: mainWindowState.height,
    icon: getAssetPath('icon.png'),
    webPreferences: {
      nodeIntegration: true,
      webviewTag: true,
    },
  });

  const winUserAgent =
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36';
  const testWinOnUnix = false; // set to true to simulate windows os, useful for testing s3 handling

  mainWindow.loadURL(
    mainHTML + startupParameter,
    testWinOnUnix ? { userAgent: winUserAgent } : {}
  );
  mainWindow.setMenuBarVisibility(true);

  mainWindow.webContents.on('did-finish-load', () => {
    if (!mainWindow) {
      throw new Error('"mainWindow" is not defined');
    }

    (global as any).splashWorkerWindow.hide();
    if (portableMode) {
      mainWindow.setTitle(mainWindow.getTitle() + ' Portable 🔌');
    }
    mainWindow.focus();
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
    try {
      (global as any).splashWorkerWindow.close();
      (global as any).splashWorkerWindow = null;
    } catch (err) {}
  });

  mainWindow.webContents.on('crashed', () => {
    const options = {
      type: 'info',
      title: 'Renderer Process Crashed',
      message: 'This process has crashed.',
      buttons: ['Reload', 'Close'],
    };

    if (!mainWindow) {
      globalShortcut.unregisterAll();
      return;
    }

    dialog.showMessageBox(mainWindow, options).then((dialogResponse) => {
      mainWindow?.hide();
      if (dialogResponse.response === 0) {
        reloadApp();
      } else {
        mainWindow?.close();
        globalShortcut.unregisterAll();
      }
    });

    (global as any).splashWorkerWindow.webContents.on('crashed', () => {
      try {
        (global as any).splashWorkerWindow.close();
        (global as any).splashWorkerWindow = null;
      } catch (err) {
        console.warn('Error closing the splash window. ' + err);
      }
      createSplashWorker();
    });

    ipcMain.on('worker', (event, arg) => {
      if ((global as any).splashWorkerWindow && arg.visibility) {
        (global as any).splashWorkerWindow.show();
      }
    });

    ipcMain.on('app-data-path-request', (event) => {
      event.returnValue = app.getPath('appData');
    });

    ipcMain.on('app-version-request', (event) => {
      event.returnValue = app.getVersion();
    });

    ipcMain.on('app-dir-path-request', (event) => {
      event.returnValue = path.join(__dirname, '');
    });

    ipcMain.on('global-shortcuts-enabled', (event, globalShortcutsEnabled) => {
      if (globalShortcutsEnabled) {
        globalShortcut.register('CommandOrControl+Shift+F', showSearch);
        globalShortcut.register('CommandOrControl+Shift+P', resumePlayback);
        globalShortcut.register('MediaPlayPause', resumePlayback);
        globalShortcut.register('CommandOrControl+Shift+N', newTextFile);
        globalShortcut.register('CommandOrControl+Shift+D', getNextFile);
        globalShortcut.register('MediaNextTrack', getNextFile);
        globalShortcut.register('CommandOrControl+Shift+A', getPreviousFile);
        globalShortcut.register('MediaPreviousTrack', getPreviousFile);
        globalShortcut.register('CommandOrControl+Shift+W', showApp);
      } else {
        globalShortcut.unregisterAll();
      }
    });

    ipcMain.on('relaunch-app', reloadApp);

    ipcMain.on('quit-application', () => {
      globalShortcut.unregisterAll();
      app.quit();
    });

    process.on('uncaughtException', (error) => {
      if (error.stack) {
        console.error('error:', error.stack);
        throw new Error(error.stack);
      }
      reloadApp();
    });

    mainWindowState.manage(mainWindow);

    function showApp() {
      if (mainWindow) {
        if (mainWindow.isMinimized()) {
          mainWindow.restore();
        }
        mainWindow.show();
      }
    }

    function showSearch() {
      if (mainWindow) {
        showApp();
        mainWindow.webContents.send('file', 'open-search');
      }
    }

    function newTextFile() {
      if (mainWindow) {
        showApp();
        mainWindow.webContents.send('file', 'new-text-file');
      }
    }

    function getNextFile() {
      if (mainWindow) {
        mainWindow.webContents.send('file', 'next-file');
      }
    }

    function getPreviousFile() {
      if (mainWindow) {
        mainWindow.webContents.send('file', 'previous-file');
      }
    }

    function resumePlayback() {
      if (mainWindow) {
        mainWindow.webContents.send('play-pause', true);
      }
    }

    function reloadApp() {
      if (mainWindow) {
        mainWindow.loadURL(mainHTML);
      }
    }
  });

  const menuBuilder = new MenuBuilder(mainWindow);
  menuBuilder.buildMenu();

  // Remove this if your app does not use auto updates
  // eslint-disable-next-line
  new AppUpdater();
};

app.on('window-all-closed', () => {
  // Respect the OSX convention of having the application in memory even
  // after all windows have been closed
  globalShortcut.unregisterAll();
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

if (process.env.E2E_BUILD === 'true') {
  // eslint-disable-next-line promise/catch-or-return
  app.whenReady().then(createWindow);
} else {
  app.on('ready', createWindow);
}

app.on('activate', () => {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (mainWindow === null) createWindow();
});
