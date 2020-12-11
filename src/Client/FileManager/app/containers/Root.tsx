import React from 'react';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'connected-react-router';
import { PersistGate } from 'redux-persist/integration/react';
import { hot } from 'react-hot-loader/root';
import { Store } from 'redux';
import { Persistor } from 'redux-persist';
import { History } from 'history';

import Routes from '../Routes';
import { AppState } from '../features/reducers';
import { configMetaActions } from '../features/config';

type Props = {
  store: Store<AppState>;
  persistor: Persistor;
  history: History;
};

const onBeforeLift = (store: Store<AppState>) => {
  store.dispatch(configMetaActions.setZoomRestore());
  const state = store.getState();
  console.log(state);
};

const Root = ({ store, persistor, history }: Props) => {
  return (
    <Provider store={store}>
      <PersistGate
        loading="null"
        onBeforeLift={() => onBeforeLift(store)}
        persistor={persistor}
      >
        <ConnectedRouter history={history}>
          <Routes />
        </ConnectedRouter>
      </PersistGate>
    </Provider>
  );
};

export default hot(Root);
