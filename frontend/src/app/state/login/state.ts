/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

export interface LoginStateInStore {
    login: LoginState;
}

export interface LoginState {
    // The current user.
    user?: User;

    // True, when authenticated.
    isAuthenticating: boolean;
}

export interface User {
    // The oidc subject.
    sub: string;

    // The name of the user.
    name: string;

    // The token of the user.
    token: string;
}
