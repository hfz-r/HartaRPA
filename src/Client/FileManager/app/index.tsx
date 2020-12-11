import React, { Fragment } from 'react';
import { render } from 'react-dom';
import { AppContainer as ReactHotAppContainer } from 'react-hot-loader';
import { history, configuredStore } from './features/configureStore';
import './app.global.css';

const { store, persistor } = configuredStore();

const AppContainer = process.env.PLAIN_HMR ? Fragment : ReactHotAppContainer;

document.addEventListener('contextmenu', event => event.preventDefault());
document.addEventListener('DOMContentLoaded', () => {
  const Root = require('./containers/Root').default;
  render(
    <AppContainer>
      <Root store={store} persistor={persistor} history={history} />
    </AppContainer>,
    document.getElementById('root')
  );
});
