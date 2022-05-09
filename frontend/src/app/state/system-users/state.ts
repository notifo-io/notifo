/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo, ListState } from '@app/framework';
import { SystemUserDto } from '@app/service';

export interface SystemUsersStateInStore {
    systemUsers: SystemUsersState;
}

export interface SystemUsersState {
    // All users.
    systemUsers: ListState<SystemUserDto>;

    // The current user.
    systemUser?: SystemUserDto;

    // True if upserting.
    upserting?: boolean;

    // The error if upserting fails.
    upsertingError?: ErrorInfo;
}
