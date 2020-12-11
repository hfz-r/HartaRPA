import * as configMetaTypes from './meta/types';

export interface ConfigState {
  meta: configMetaTypes.ConfigMetaState;
}

export type ConfigAction = configMetaTypes.MetaAction;
