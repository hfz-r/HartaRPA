declare module '*.json';
declare module '*.md';
declare module '*.txt';
declare module '*.png';
declare module '*.svg';
declare module '*.woff';
declare module '*.woff2';
declare module '*.xml';

declare interface Window {
  // interface TSCustomWindow extends Window {
  walkCanceled?: boolean;
  ExtLogoURL?: string;
  ExtDefaultVerticalPanel?: string;
  ExtDisplayMode?: string;
  ExtDefaultPerspective?: string;
  ExtLocationsReadOnly?: string;
  ExtTagLibrary?: Array<any>;
  ExtLocations?: Array<any>;
  ExtTheme?: string;
  ExtIsFirstRun?: boolean;
  __REDUX_DEVTOOLS_EXTENSION_COMPOSE__?: any;
}

declare interface NodeModule {
  /** https://webpack.js.org/api/hot-module-replacement */
  hot: {
    accept(file: string, update: () => void): void;
  };
}

declare interface Document {
  webkitExitFullscreen: any;
}
