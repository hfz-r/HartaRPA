import * as types from './types';

const META_INITIAL_STATE: types.ConfigMetaState = {
  zoomFactor: 1,
};

export function metaReducer(
  state: types.ConfigMetaState = META_INITIAL_STATE,
  action: types.MetaAction
): types.ConfigMetaState {
  switch (action.type) {
    case types.ConfigMetaActions.SET_ZOOM_RESTORE:
      return state;
    default: {
      return state;
    }
  }
}
