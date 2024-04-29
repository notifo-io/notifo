/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { SystemUserDto } from '@app/service';

export interface SystemUsersStateInStore {
    systemUsers: SystemUsersState;
}

export interface SystemUsersState {
    // All users.
    systemUsers: ListState<SystemUserDto>;

    // Mutation for upserting.
    upserting?: MutationState;
}
