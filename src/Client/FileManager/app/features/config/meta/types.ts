export interface ConfigMetaState {
  zoomFactor: number;
}

export enum ConfigMetaActions {
  SET_ZOOM_RESTORE = 'CONFIG/SET_ZOOM_RESTORE',
}

export interface SetZoomRestoreAction {
  type: ConfigMetaActions.SET_ZOOM_RESTORE;
}

export type MetaAction = SetZoomRestoreAction;
