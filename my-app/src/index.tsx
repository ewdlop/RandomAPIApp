import React from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { Route, Switch } from 'react-router' // react-router v4/v5
import { ConnectedRouter } from 'connected-react-router'
import { store, history } from './app/store';
import dotenv from 'dotenv';
import App from './App';
import reportWebVitals from './reportWebVitals';
import './index.css';

//dotenv.config();
const container = document.getElementById('root')!;
const root = createRoot(container);
const localStore = 
root.render(
  <React.StrictMode>
    <Provider store={store}>
      {/* <ConnectedRouter history={history}>
        <Route path="/" component={App}>
          <Route path="foo" component={Foo}/>
          <Route path="bar" component={Bar}/>
        </Route>
        </ConnectedRouter> */}
      <App />
    </Provider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
