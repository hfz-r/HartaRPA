import { combineReducers } from 'redux';
import { persistReducer } from 'redux-persist';
import { connectRouter } from 'connected-react-router';
import { History } from 'history';
import storage from 'redux-persist/lib/storage';

import { ConfigState } from './config/types';
import { configReducer } from './config/reducer';

const rootPersistConfig = {
  key: 'root',
  storage,
  version: 2,
  blacklist: [
    'app',
    'locationIndex',
    window.ExtLocations || false ? 'locations' : '',
    window.ExtTagLibrary || false ? 'taglibrary' : '',
  ],
  debug: false,
};

export interface AppState {
  config: ConfigState;
  router: any;
}

export function createRootReducer(history: History): any {
  const rootReducer = combineReducers<AppState>({
    config: configReducer,
    router: connectRouter(history),
  });
  return persistReducer(rootPersistConfig, rootReducer);
}
