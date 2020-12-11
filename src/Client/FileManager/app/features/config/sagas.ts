import { SagaIterator } from 'redux-saga';
import { call, put, takeLatest } from 'redux-saga/effects';

import * as configMetaTypes from './meta/types';

export function* testSaga(): SagaIterator {
  console.log("Hello!")
}

export function* configSaga(): SagaIterator {
  yield takeLatest(configMetaTypes.ConfigMetaActions.SET_ZOOM_RESTORE, testSaga);
}
