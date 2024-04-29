/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { UserDto } from '@app/service';

export interface UsersStateInStore {
    users: UsersState;
}

export interface UsersState {
    // All users.
    users: ListState<UserDto>;

    // The current user.
    user?: UserDto;

    // Mutation for loading user.
    loadingUser?: MutationState;

    // Mutation for upserting.
    upserting?: MutationState;
}
