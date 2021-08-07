/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, ListState } from '@app/framework';
import { UserDto } from '@app/service';

export interface UsersStateInStore {
    users: UsersState;
}

export interface UsersState {
    // All users.
    users: ListState<UserDto>;

    // The current user.
    user?: UserDto;

    // True if loading user.
    loadingUser?: boolean;

    // The user loading error.
    loadingUsersError?: any;

    // True if upserting.
    upserting?: boolean;

    // The error if upserting fails.
    upsertingError?: ErrorDto;

    // True if deleting.
    deleting?: boolean;

    // The error if deleting fails.
    deletingError?: ErrorDto;
}
