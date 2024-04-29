/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createExtendedReducer, createList } from '@app/framework';
import { Clients } from '@app/service';
import { selectApp } from './../shared';
import { NotificationsState, NotificationsStateInStore } from './state';

export const loadNotifications = createList<NotificationsState, NotificationsStateInStore>('notifications', 'notifications').with({
    name: 'notifications/load',
    queryFn: async (p: { appId: string; userId: string; channels: string[] }, q) => {
        const { items, total } = await Clients.Notifications.getNotifications(p.appId, p.userId, p.channels, undefined, undefined, q.search, q.take, q.skip);

        return { items, total };
    },
});

const initialState: NotificationsState = {
    notifications: loadNotifications.createInitial(),
};

const operations = [
    loadNotifications,
];

export const notificationsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }), operations);
