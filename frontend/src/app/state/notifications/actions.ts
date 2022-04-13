/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { listThunk, Query } from '@app/framework';
import { Clients, UserNotificationDetailsDto } from '@app/service';
import { selectApp } from './../shared';
import { NotificationsState } from './state';

const list = listThunk<NotificationsState, UserNotificationDetailsDto>('notifications', 'notifications', async (params) => {
    const { items, total } = await Clients.Notifications.getNotifications(params.appId, params.userId, params.channels, params.search, params.take, params.skip);

    return { items, total };
});

export const loadNotifications = (appId: string, userId: string, query?: Partial<Query>, reset = false, channels?: string[]) => {
    return list.action({ appId, userId, query, reset, channels });
};

const initialState: NotificationsState = {
    notifications: list.createInitial(),
};

export const notificationsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
