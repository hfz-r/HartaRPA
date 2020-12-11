import fsextra from 'fs-extra';
import pathLib from 'path';
import winattr from 'winattr';
import AppConfig from '-/config';
import { getMetaDirectoryPath } from '../utils/paths';
import TrayIcon from '../../resources/icon.png';
import TrayIcon2x from '../../resources/icons/icon@2x.png';
import TrayIcon3x from '../../resources/icons/icon@3x.png';

export default class ElectronIO {
  electron: any;

  win: any;

  app: any;

  ipcRenderer: any;

  remote: any;

  workerWindow: any;

  pathUtils: any;

  fs: any;

  fsWatcher: any;

  webFrame: any;

  tsTray: any;

  constructor() {
    if (window.require) {
      this.electron = window.require('electron');
      this.ipcRenderer = this.electron.ipcRenderer;
      this.webFrame = this.electron.webFrame;
      this.remote = this.electron.remote;
      this.workerWindow = this.remote.getGlobal('splashWorkerWindow');
      this.win = this.remote.getCurrentWindow();
      this.app = this.remote.app;
      this.fs = fsextra; // window.require('fs-extra');
      this.pathUtils = window.require('path');
    }
  }

  initMainMenu = (menuConfig: Array<Object>) => {
    const { Menu } = this.remote;
    const defaultMenu = Menu.buildFromTemplate(menuConfig);
    Menu.setApplicationMenu(defaultMenu);
  };

  initTrayMenu = (menuConfig: Array<Object>): void => {
    if (process.platform === 'darwin') {
      return;
    }

    const mainWindow = this.win;
    const { Menu } = this.remote;
    const { Tray } = this.remote;
    const { nativeImage } = this.remote;

    let nImage;
    if (process.platform === 'win32') {
      nImage = nativeImage.createFromDataURL(TrayIcon2x);
      // @ts-ignore
    } else if (process.platform === 'darwin') {
      nImage = nativeImage.createFromDataURL(TrayIcon);
      nImage.addRepresentation({ scaleFactor: 2.0, dataURL: TrayIcon2x });
      nImage.addRepresentation({ scaleFactor: 3.0, dataURL: TrayIcon3x });
    } else {
      nImage = nativeImage.createFromDataURL(TrayIcon2x);
    }

    if (this.tsTray && this.tsTray.destroy) {
      this.tsTray.destroy();
    }

    this.tsTray = new Tray(nImage);

    this.tsTray.on('click', () => {
      if (mainWindow) {
        mainWindow.show();
      }
    });

    const trayMenu = Menu.buildFromTemplate(menuConfig);

    this.tsTray.setToolTip('Harta RPA-FM');
    this.tsTray.setContextMenu(trayMenu);
  };

  isWorkerAvailable = (): boolean => {
    let workerAvailable = false;
    try {
      if (this.workerWindow && this.workerWindow.webContents) {
        workerAvailable = true;
      }
    } catch (err) {
      console.info('Error by finding if worker is available.');
    }
    return workerAvailable;
  };

  showMainWindow = (): void => {
    if (this.win.isMinimized()) {
      this.win.restore();
    }
    this.win.show();
  };

  quitApp = (): void => {
    this.ipcRenderer.send('quit-application', 'Bye, bye...');
  };

  watchDirectory = (dirPath: string, listener: Object): void => {
    this.fsWatcher = this.fs.watch(
      dirPath,
      { persistent: true, recursive: false },
      listener
    );
  };

  focusWindow = (): void => {
    this.win.focus();
  };

  getDevicePaths = (): Object => {
    const paths = {
      // Desktop: this.app.getPath('desktop'),
      // Documents: this.app.getPath('documents'),
      Downloads: this.app.getPath('downloads'),
      // Music: this.app.getPath('music'),
      // Pictures: this.app.getPath('pictures'),
      // Videos: this.app.getPath('videos')
    };
    return paths;
  };

  getUserHomePath = (): string => this.app.getPath('home');

  getAppDataPath = (): string =>
    this.ipcRenderer.sendSync('app-data-path-request', 'notNeededArgument');

  setZoomFactorElectron = (zoomLevel: number) =>
    this.webFrame.setZoomFactor(zoomLevel);

  setGlobalShortcuts = (globalShortcutsEnabled: boolean) => {
    this.ipcRenderer.send('global-shortcuts-enabled', globalShortcutsEnabled);
  };

  getAppPath = (): string =>
    this.ipcRenderer.sendSync('app-dir-path-request', 'notNeededArgument');

  createDirectoryTree = (directoryPath: string): Object => {
    console.log('Creating directory index for: ' + directoryPath);
    const generateDirectoryTree = (dirPath: string) => {
      try {
        const tree: any = {};
        const dstats = this.fs.lstatSync(dirPath);
        tree.name = this.pathUtils.basename(dirPath);
        tree.isFile = false;
        tree.lmdt = dstats.mtime;
        tree.path = dirPath;
        tree.children = [];

        const dirList = this.fs.readdirSync(dirPath);
        for (let i = 0; i < dirList.length; i += 1) {
          const path = dirPath + AppConfig.dirSeparator + dirList[i];
          const stats = this.fs.lstatSync(path);
          if (stats.isFile()) {
            tree.children.push({
              name: this.pathUtils.basename(path),
              isFile: true,
              size: stats.size,
              lmdt: stats.mtime,
              path,
            });
          } else {
            tree.children.push(generateDirectoryTree(path));
          }
        }
        return tree;
      } catch (ex) {
        console.error('Generating tree for ' + dirPath + ' failed ' + ex);
      }
    };
    return generateDirectoryTree(directoryPath);
  };

  createDirectoryIndexInWorker = (
    directoryPath: string,
    extractText: boolean
  ): Promise<any> =>
    new Promise((resolve, reject) => {
      if (this.isWorkerAvailable()) {
        const timestamp = new Date().getTime().toString();
        this.workerWindow.webContents.send('worker', {
          id: timestamp,
          action: 'createDirectoryIndex',
          path: directoryPath,
          extractText,
        });
        this.ipcRenderer.once(timestamp, (data: any) => {
          resolve(data.result);
        });
      } else {
        reject('Worker window not available!');
      }
    });

  createThumbnailsInWorker = (tmbGenerationList: Array<string>): Promise<any> =>
    new Promise((resolve, reject) => {
      if (this.isWorkerAvailable()) {
        const timestamp = new Date().getTime().toString();
        this.workerWindow.webContents.send('worker', {
          id: timestamp,
          action: 'createThumbnails',
          tmbGenerationList,
        });
        this.ipcRenderer.once(timestamp, (data: any) => {
          resolve(data.result);
        });
      } else {
        reject('Worker window not available!');
      }
    });

  listDirectoryPromise = (
    path: string,
    lite: boolean = true
  ): Promise<Array<Object>> =>
    new Promise((resolve) => {
      const enhancedEntries: Array<any> = [];
      let entryPath;
      let stats;
      let metaFolderPath: any;
      let eentry: any;
      let containsMetaFolder = false;

      if (path.startsWith('./')) {
        path = pathLib.resolve(path);
      }
      this.fs.readdir(path, (error: any, entries: any) => {
        if (error) {
          console.warn('Error listing directory ' + path);
          resolve(enhancedEntries);
          return;
        }

        if (window.walkCanceled) {
          resolve(enhancedEntries);
          return;
        }

        if (entries) {
          entries.forEach((entry: any) => {
            entryPath = path + AppConfig.dirSeparator + entry;
            eentry = {};
            eentry.name = entry;
            eentry.path = entryPath;
            eentry.tags = [];
            eentry.thumbPath = '';
            eentry.meta = {};

            try {
              stats = this.fs.statSync(entryPath);
              eentry.isFile = stats.isFile();
              eentry.size = stats.size;
              eentry.lmdt = stats.mtime.getTime();

              if (
                !eentry.isFile &&
                eentry.name.endsWith(AppConfig.metaFolder)
              ) {
                containsMetaFolder = true;
              }

              // Read tsm.json from sub folders
              if (!eentry.isFile && !lite) {
                const folderMetaPath =
                  eentry.path +
                  AppConfig.dirSeparator +
                  AppConfig.metaFolder +
                  AppConfig.dirSeparator +
                  AppConfig.metaFolderFile;
                try {
                  const folderMeta = this.fs.readJsonSync(folderMetaPath);
                  eentry.meta = folderMeta;
                } catch (err) {}

                // Loading thumbs for folders
                if (!eentry.path.includes('/' + AppConfig.metaFolder)) {
                  const folderTmbPath =
                    eentry.path +
                    AppConfig.dirSeparator +
                    AppConfig.metaFolder +
                    AppConfig.dirSeparator +
                    AppConfig.folderThumbFile;
                  const tmbStats = this.fs.statSync(folderTmbPath);
                  if (tmbStats.isFile()) {
                    eentry.thumbPath = folderTmbPath;
                  }
                }
              }

              if (window.walkCanceled) {
                resolve(enhancedEntries);
                return;
              }
            } catch (e) {
              console.warn(
                'Can not load properties for: ' + entryPath + ' ' + e
              );
            }
            enhancedEntries.push(eentry);
          });

          // Read the .ts meta content
          if (!lite && containsMetaFolder) {
            metaFolderPath = getMetaDirectoryPath(path, AppConfig.dirSeparator);
            this.fs.readdir(metaFolderPath, (err: any, metaEntries: any) => {
              if (err) {
                console.log(
                  'Error listing meta directory ' + metaFolderPath + ' - ' + err
                );
                resolve(enhancedEntries);
                return;
              }

              if (window.walkCanceled) {
                resolve(enhancedEntries);
                return;
              }

              if (metaEntries) {
                metaEntries.forEach((metaEntryName: any) => {
                  // Reading meta json files with tags and description
                  if (metaEntryName.endsWith(AppConfig.metaFileExt)) {
                    const fileNameWithoutMetaExt = metaEntryName.substr(
                      0,
                      metaEntryName.lastIndexOf(AppConfig.metaFileExt)
                    );
                    const origFile = enhancedEntries.find(
                      (result) => result.name === fileNameWithoutMetaExt
                    );
                    if (origFile) {
                      const metaFilePath =
                        metaFolderPath + AppConfig.dirSeparator + metaEntryName;
                      const metaFileObj = this.fs.readJsonSync(metaFilePath);
                      if (metaFileObj) {
                        enhancedEntries.map((enhancedEntry) => {
                          if (enhancedEntry.name === fileNameWithoutMetaExt) {
                            enhancedEntry.meta = metaFileObj;
                          }
                          return true;
                        });
                      }
                    }
                  }

                  // Finding if thumbnail available
                  if (metaEntryName.endsWith(AppConfig.thumbFileExt)) {
                    const fileNameWithoutMetaExt = metaEntryName.substr(
                      0,
                      metaEntryName.lastIndexOf(AppConfig.thumbFileExt)
                    );
                    enhancedEntries.map((enhancedEntry) => {
                      if (enhancedEntry.name === fileNameWithoutMetaExt) {
                        const thumbFilePath =
                          metaFolderPath +
                          AppConfig.dirSeparator +
                          metaEntryName;
                        enhancedEntry.thumbPath = thumbFilePath;
                      }
                      return true;
                    });
                  }

                  if (window.walkCanceled) {
                    resolve(enhancedEntries);
                  }
                });
              }
              resolve(enhancedEntries);
            });
          } else {
            resolve(enhancedEntries);
          }
        }
      });
    });

  getPropertiesPromise = (path: string): Promise<any> =>
    new Promise((resolve) => {
      this.fs.lstat(path, (err: any, stats: any) => {
        if (err) {
          resolve(false);
          return;
        }

        if (stats) {
          resolve({
            name: path.substring(
              path.lastIndexOf(AppConfig.dirSeparator) + 1,
              path.length
            ),
            isFile: stats.isFile(),
            size: stats.size,
            lmdt: stats.mtime.getTime(),
            path,
          });
        }
      });
    });
}
