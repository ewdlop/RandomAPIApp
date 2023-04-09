import React from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { store, history } from './app/store';
import dotenv from 'dotenv';
import App from './App';
import reportWebVitals from './reportWebVitals';
import './index.css';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";
import Movie from './Movie';
//dotenv.config();
const container = document.getElementById('root')!;
const root = createRoot(container);
const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
  },
  {
    path: "/movie",
    element: <Movie />,
  },
]);

const setToken = (token: string) => {
  localStorage.setItem('token', token);
};

const getToken = () => {
  return localStorage.getItem('token');
};

const jwt = process.env.REACT_APP_TOKEN || '';
setToken(jwt);

root.render(
  <React.StrictMode>
    <Provider store={store}>
      <RouterProvider router={router} />
    </Provider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
