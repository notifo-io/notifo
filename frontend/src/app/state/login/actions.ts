/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { AuthService } from '@app/service';
import { routerActions } from 'react-router-redux';
import { Dispatch, Middleware, Reducer } from 'redux';
import { LoginState } from './state';

export const LOGIN_LOGIN_STARTED = 'LOGIN_LOGIN_STARTED';
export const LOGIN_LOGIN_SUCCEEEDED = 'LOGIN_LOGIN_SUCCEEEDED';
export const LOGIN_LOGIN_SUCCEEEDED_REDIRECT = 'LOGIN_LOGIN_SUCCEEEDED_REDIRECT';
export const LOGIN_LOGOUT_STARTED = 'LOGIN_LOGOUT_STARTED';
export const LOGIN_LOGOUT_SUCCEEEDED = 'LOGIN_LOGOUT_SUCCEEEDED';
export const LOGIN_LOGOUT_SUCCEEEDED_REDIRECT = 'LOGIN_LOGOUT_SUCCEEEDED_REDIRECT';

function getUser(user: Oidc.User) {
    return { sub: user.profile.sub, name: user.profile.name, token: user.access_token };
}

export const loginStartAsync = () => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: LOGIN_LOGIN_STARTED });

        const userManager = AuthService.getUserManager();

        const currentUser = await userManager.getUser();

        if (!currentUser) {
            await userManager.signinRedirect();
        } else {
            const user = getUser(currentUser);

            dispatch({ type: LOGIN_LOGIN_SUCCEEEDED, user });
        }
    };
};

export const loginDoneAsync = () => {
    return async (dispatch: Dispatch) => {
        const userManager = AuthService.getUserManager();

        const currentUser = await userManager.signinCallback();

        if (currentUser) {
            const user = getUser(currentUser);

            dispatch({ type: LOGIN_LOGIN_SUCCEEEDED_REDIRECT, user });
        }
    };
};

export const logoutStartAsync = () => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: LOGIN_LOGOUT_STARTED });

        const userManager = AuthService.getUserManager();

        const currentUser = await userManager.getUser();

        if (currentUser) {
            await userManager.signoutRedirect();
        }
    };
};

export const logoutDoneAsync = () => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: LOGIN_LOGOUT_STARTED });

        const userManager = AuthService.getUserManager();

        const response = await userManager.signoutRedirectCallback();

        if (!response.error) {
            dispatch({ type: LOGIN_LOGOUT_SUCCEEEDED_REDIRECT });
        }
    };
};

export function loginMiddleware(): Middleware {
    const middleware: Middleware = state => next => action => {
        if (action.error?.statusCode === 401) {
            const userManager = AuthService.getUserManager();

            userManager.signoutRedirect();
        }

        const result = next(action);

        switch (action.type) {
            case LOGIN_LOGIN_SUCCEEEDED_REDIRECT:
                state.dispatch(routerActions.push('/'));
                break;
            case LOGIN_LOGOUT_SUCCEEEDED_REDIRECT:
                state.dispatch(routerActions.push('/'));
                break;
        }

        return result;
    };

    return middleware;
}

export function loginReducer(): Reducer<LoginState> {
    const initialState = { isAuthenticating: true };

    const reducer: Reducer<LoginState> = (state = initialState, action) => {
        switch (action.type) {
            case LOGIN_LOGIN_STARTED:
                return { ...state, isAuthenticating: true };
            case LOGIN_LOGIN_SUCCEEEDED:
            case LOGIN_LOGIN_SUCCEEEDED_REDIRECT:
                return { ...state, isAuthenticating: false, user: action.user };
            default:
                return state;
        }
    };

    return reducer;
}
