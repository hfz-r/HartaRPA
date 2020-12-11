export const AppConfig = {
  tagspacesAppPath: '/hartafm/',
  metaFolder: '.ts',
  metaFolderFile: 'tsm.json',
  folderIndexFile: 'tsi.json',
  folderThumbFile: 'tst.jpg',
  metaFileExt: '.json',
  thumbFileExt: '.jpg',
  thumbType: 'image/jpeg',
  contentFileExt: '.txt',
  beginTagContainer: '[',
  endTagContainer: ']',
  tagDelimiter: ' ',
  prefixTagContainer: '',
  maxCollectedTag: 500,
  maxThumbSize: 400,
  thumbBgColor: '#FFFFFF',
  indexerLimit: 200000,
  // maxSearchResult: 1000,
  defaultFileColor: '#808080',
  defaultFolderColor: '#555', // transparent #FDEEBD #ff791b #2c001e #880e4f
  isElectron: navigator.userAgent.toLowerCase().indexOf(' electron/') > -1,
  isFirefox: navigator.userAgent.toLowerCase().includes('firefox'), // typeof InstallTrigger !== 'undefined';
  isWin: navigator.appVersion.includes('Win'),
  isMacLike: navigator.userAgent.match(/(Mac|iPhone|iPod|iPad)/i),
  isWeb:
    document.URL.startsWith('http') &&
    !document.URL.startsWith('http://localhost:1212/'),
  dirSeparator:
    navigator.appVersion.includes('Win') && !document.URL.startsWith('http')
      ? '\\'
      : '/',
};

export default AppConfig;
