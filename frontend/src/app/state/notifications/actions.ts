/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { listThunk, Query } from '@app/framework';
import { Clients, UserNotificationDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { selectApp } from './../shared';
import { NotificationsState } from './state';

const list = listThunk<NotificationsState, UserNotificationDto>('notifications', 'notifications', async (params) => {
    const { items, total } = await Clients.Notifications.getNotifications(params.appId, params.userId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadNotifications = (appId: string, userId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, userId, query, reset });
};

const initialState: NotificationsState = {
    notifications: list.createInitial(),
};

export const notificationsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
