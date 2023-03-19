import { createBrowserHistory } from 'history'
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit';
import { routerMiddleware } from 'connected-react-router'
import counterReducer from '../features/counter/counterSlice';


export const history = createBrowserHistory()

export const store = configureStore({
  reducer: {
    counter: counterReducer,
  },
});

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
