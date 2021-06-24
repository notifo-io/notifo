/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState } from '@app/framework';
import { UserNotificationDto } from '@app/service';

export interface NotificationsStateInStore {
    notifications: NotificationsState;
}

export interface NotificationsState {
    // All subscriptions.
    notifications: ListState<UserNotificationDto>;
}
