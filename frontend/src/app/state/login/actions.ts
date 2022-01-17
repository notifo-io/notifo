/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createAction, createReducer } from '@reduxjs/toolkit';
import { routerActions } from 'react-router-redux';
import { Dispatch, Middleware } from 'redux';
import { AuthService } from '@app/service';
import { LoginState, User } from './state';

const loginStarted = createAction('login/started');
const loginDoneSilent = createAction<{ user: User }>('login/done/silent');
const loginDoneRedirect = createAction<{ user: User }>('login/done/redirect');
const loginFailed = createAction('login/failed');

const logoutStarted = createAction('logout/started');
const logoutDoneRedirect = createAction('logout/redirect');

export const loginStart = () => {
    return async (dispatch: Dispatch) => {
        dispatch(loginStarted());

        const userManager = AuthService.getUserManager();

        let currentUser = await userManager.getUser();

        if (!currentUser) {
            try {
                currentUser = await userManager.signinSilent();
            } catch {
                currentUser = null;
            }
        }

        if (!currentUser) {
            await userManager.signinRedirect();
        } else {
            const user = getUser(currentUser);

            dispatch(loginDoneSilent({ user }));
        }
    };
};

export const loginDone = () => {
    return async (dispatch: Dispatch) => {
        const userManager = AuthService.getUserManager();

        const currentUser = await userManager.signinCallback();

        if (!currentUser) {
            dispatch(loginFailed());
        } else if (currentUser) {
            const user = getUser(currentUser);

            dispatch(loginDoneRedirect({ user }));
        }
    };
};

export const logoutStart = () => {
    return async (dispatch: Dispatch) => {
        dispatch(logoutStarted());

        const userManager = AuthService.getUserManager();

        const currentUser = await userManager.getUser();

        if (currentUser) {
            await userManager.signoutRedirect();
        }
    };
};

export const logoutDone = () => {
    return async (dispatch: Dispatch) => {
        const userManager = AuthService.getUserManager();

        const response = await userManager.signoutRedirectCallback();

        if (!response.error) {
            dispatch(logoutDoneRedirect());
        }
    };
};

export const loginMiddleware: Middleware = (state) => next => action => {
    if (action.payload?.statusCode === 401 || action.payload?.error?.statusCode === 401) {
        const userManager = AuthService.getUserManager();

        userManager.signoutRedirect();
    }

    const result = next(action);

    if (loginDoneRedirect.match(action) || logoutDoneRedirect.match(action) || loginFailed.match(action)) {
        state.dispatch(routerActions.push('/'));
    }

    return result;
};

const initialState: LoginState = {};

export const loginReducer = createReducer(initialState, builder => builder
    .addCase(loginStarted, (state) => {
        state.isAuthenticating = true;
        state.isAuthenticated = false;
    })
    .addCase(loginDoneSilent, (state, action) => {
        state.isAuthenticating = false;
        state.isAuthenticated = true;
        state.user = action.payload.user;
    })
    .addCase(loginDoneRedirect, (state, action) => {
        state.isAuthenticating = false;
        state.isAuthenticated = true;
        state.user = action.payload.user;
    })
    .addCase(loginFailed, (state) => {
        state.isAuthenticating = false;
        state.isAuthenticated = true;
        state.user = undefined;
    }));

function getUser(user: Oidc.User): User {
    const { sub, name, role } = user.profile!;

    return { sub, name, role, token: user.access_token } as any;
}
