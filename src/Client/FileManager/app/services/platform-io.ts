// @ts-ignore
import NativePlatformIO from './_PLATFORMIO_';
import AppConfig from '-/config';

const nativeAPI: any = new NativePlatformIO();

export default class PlatformIO {
  static getDirSeparator = (): string => AppConfig.dirSeparator;

  static initMainMenu = (menuConfig: Array<Object>): void => {
    if (nativeAPI.initMainMenu) {
      nativeAPI.initMainMenu(menuConfig);
    } else {
      console.log('initMainMenu not supported');
    }
  };

  static initTrayMenu = (menuConfig: Array<Object>): void => {
    if (nativeAPI.initTrayMenu) {
      nativeAPI.initTrayMenu(menuConfig);
    } else {
      console.log('initTrayMenu not supported');
    }
  };

  static isWorkerAvailable = (): boolean => nativeAPI.isWorkerAvailable();

  static setZoomFactorElectron = (zoomLevel: any) => {
    if (nativeAPI.setZoomFactorElectron) {
      nativeAPI.setZoomFactorElectron(zoomLevel);
    } else {
      console.log('setZoomFactorElectron not supported');
    }
  };

  static setGlobalShortcuts = (globalShortcutsEnabled: any) => {
    if (nativeAPI.setGlobalShortcuts) {
      nativeAPI.setGlobalShortcuts(globalShortcutsEnabled);
    } else {
      console.log('setGlobalShortcuts not supported');
    }
  };

  static showMainWindow = (): void => nativeAPI.showMainWindow();

  static quitApp = (): void => nativeAPI.quitApp();

  static watchDirectory = (dirPath: string, listener: any): void =>
    nativeAPI.watchDirectory(dirPath, listener);

  static focusWindow = (): void => nativeAPI.focusWindow();

  static getDevicePaths = (): Object => nativeAPI.getDevicePaths();

  static getAppDataPath = (): string => nativeAPI.getAppDataPath();

  static getUserHomePath = (): string => nativeAPI.getUserHomePath();

  static createDirectoryTree = (directoryPath: string): Object =>
    nativeAPI.createDirectoryTree(directoryPath);

  static createDirectoryIndexInWorker = (
    directoryPath: string,
    extractText: boolean
  ): Promise<any> =>
    nativeAPI.createDirectoryIndexInWorker(directoryPath, extractText);

  static createThumbnailsInWorker = (
    tmbGenerationList: Array<string>
  ): Promise<any> => nativeAPI.createThumbnailsInWorker(tmbGenerationList);

  static listDirectoryPromise = (
    path: string,
    lite: boolean = true,
    extractText: boolean = true
  ): Promise<Array<any>> => {
    return nativeAPI.listDirectoryPromise(path, lite, extractText);
  };

  static getPropertiesPromise = (path: string): Promise<any> => {
    return nativeAPI.getPropertiesPromise(path);
  };

  static createDirectoryPromise = (dirPath: string): Promise<any> => {
    return nativeAPI.createDirectoryPromise(dirPath);
  };

  static copyFilePromise = (
    sourceFilePath: string,
    targetFilePath: string
  ): Promise<any> => {
    return nativeAPI.copyFilePromise(sourceFilePath, targetFilePath);
  };

  static renameFilePromise = (
    filePath: string,
    newFilePath: string
  ): Promise<any> => {
    return nativeAPI.renameFilePromise(filePath, newFilePath);
  };

  static renameDirectoryPromise = (
    dirPath: string,
    newDirName: string
  ): Promise<any> => {
    return nativeAPI.renameDirectoryPromise(dirPath, newDirName);
  };

  static loadTextFilePromise = (
    filePath: string,
    isPreview?: boolean
  ): Promise<any> => {
    return nativeAPI.loadTextFilePromise(filePath, isPreview);
  };

  static getFileContentPromise = (
    filePath: string,
    type?: string
  ): Promise<Object> => {
    return nativeAPI.getFileContentPromise(filePath, type);
  };

  static saveFilePromise = (
    filePath: string,
    content: any,
    overwrite: boolean
  ): Promise<any> => {
    return nativeAPI.saveFilePromise(filePath, content, overwrite);
  };

  static saveTextFilePromise = (
    filePath: string,
    content: string,
    overwrite: boolean
  ): Promise<any> => {
    return nativeAPI.saveTextFilePromise(filePath, content, overwrite);
  };

  static saveBinaryFilePromise = (
    filePath: string,
    content: any,
    overwrite: boolean
  ): Promise<any> => {
    return nativeAPI.saveBinaryFilePromise(filePath, content, overwrite);
  };

  static deleteFilePromise = (
    path: string,
    useTrash?: boolean
  ): Promise<any> => {
    return nativeAPI.deleteFilePromise(path, useTrash);
  };

  static deleteDirectoryPromise = (
    path: string,
    useTrash?: boolean
  ): Promise<any> => {
    return nativeAPI.deleteDirectoryPromise(path, useTrash);
  };

  static openDirectory = (dirPath: string): void =>
    nativeAPI.openDirectory(dirPath);

  static showInFileManager = (dirPath: string): void =>
    nativeAPI.showInFileManager(dirPath);

  static openFile = (filePath: string): void => nativeAPI.openFile(filePath);

  static openUrl = (url: string): void => nativeAPI.openUrl(url);

  static selectFileDialog = (): Promise<any> => nativeAPI.selectFileDialog();

  static selectDirectoryDialog = (): Promise<any> =>
    nativeAPI.selectDirectoryDialog();
}
