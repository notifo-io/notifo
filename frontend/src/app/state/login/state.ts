/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { AdminProfileDto } from '@app/service';

export interface LoginStateInStore {
    login: LoginState;
}

export interface LoginState {
    // The current user.
    user?: User;

    // The profile.
    profile?: AdminProfileDto;

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
    roles: ReadonlyArray<string>;
}
