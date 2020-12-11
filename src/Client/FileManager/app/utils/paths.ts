import AppConfig from '-/config';

export function getMetaDirectoryPath(
  directoryPath: string,
  dirSeparator: string // = AppConfig.dirSeparator
) {
  return (
    (directoryPath ? normalizePath(directoryPath) + dirSeparator : '') +
    AppConfig.metaFolder
  );
}

export function cleanTrailingDirSeparator(dirPath: string): string {
  if (dirPath) {
    if (dirPath.lastIndexOf('\\') === dirPath.length - 1) {
      return dirPath.substring(0, dirPath.length - 1);
    }
    if (dirPath.lastIndexOf('/') === dirPath.length - 1) {
      return dirPath.substring(0, dirPath.length - 1);
    }
    return dirPath;
  }
  // console.log('Directory Path ' + dirPath + ' undefined');
  return '';
}

export function normalizePath(path: string): string {
  return cleanTrailingDirSeparator(path.replace(/\/\//g, '/'));
}
