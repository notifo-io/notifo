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

    // True, when authenticating.
    isAuthenticating?: boolean;

    // True, when authenticated.
    isAuthenticated?: boolean;
}

export interface User {
    // The oidc subject.
    sub: string;

    // The name of the user.
    name: string;

    // The token of the user.
    token: string;

    // The role.
    role?: string;
}
