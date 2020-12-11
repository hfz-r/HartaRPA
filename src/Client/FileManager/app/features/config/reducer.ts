import { combineReducers } from 'redux';

import * as configMetaReducer from './meta/reducer';
import * as types from './types';

export const configReducer = combineReducers<types.ConfigState>({
  meta: configMetaReducer.metaReducer,
});
