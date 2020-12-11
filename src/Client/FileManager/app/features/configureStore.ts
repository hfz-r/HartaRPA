import { createStore, applyMiddleware, compose } from 'redux';
import { persistStore } from 'redux-persist';
import { createHashHistory } from 'history';
import { routerMiddleware, routerActions } from 'connected-react-router';
import { createLogger } from 'redux-logger';
import createSagaMiddleware from 'redux-saga';

import { createRootReducer, AppState } from './reducers';
import sagas from './sagas';
import onlineListener from '../services/onlineListener';

const history = createHashHistory();
const rootReducer = createRootReducer(history);

const configuredStore = (initialState?: AppState) => {
  // router middleware
  const router = routerMiddleware(history);
  const middleware = [router];
  // Redux DevTools
  const composeEnhancer = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__
    ? window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__({
        ...routerActions,
        serialize: true,
      })
    : compose;

  const shouldIncludeLogger = !['test', 'production'].includes(
    process.env.NODE_ENV || ''
  );
  // Logger middleware
  if (shouldIncludeLogger) {
    const logger = createLogger({
      level: 'info',
      collapsed: true,
    });
    middleware.push(logger);
  }
  // Saga middleware
  const sagaMiddleware = createSagaMiddleware();
  middleware.push(sagaMiddleware);

  const enhancers = [applyMiddleware(...middleware)];
  const store = createStore(
    rootReducer,
    initialState,
    composeEnhancer(...enhancers)
  );

  onlineListener(store.dispatch);

  Object.keys(sagas).forEach((saga) => {
    sagaMiddleware.run(sagas[saga as keyof typeof sagas]);
  });

  const persistor = persistStore(store);

  if (module.hot) {
    module.hot.accept('./reducers', () =>
      store.replaceReducer(require('./reducers'))
    );
  }

  return { store, persistor };
};

export { configuredStore, history };
