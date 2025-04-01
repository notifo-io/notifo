/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { toast } from 'react-toastify';
import { Middleware } from 'redux';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, SubscribeDto } from '@app/service';
import { selectApp } from './../shared';
import { SubscriptionsState, SubscriptionsStateInStore } from './state';

export const loadSubscriptions = createList<SubscriptionsState, SubscriptionsStateInStore>('subscriptions', 'subscriptions').with({
    name: 'subscriptions/load',
    queryFn: async (p: { appId: string; userId: string }, q) => {
        const { items, total } = await Clients.Users.getSubscriptions(p.appId, p.userId, q.search, q.take, q.skip);

        return { items, total };
    },
});

export const upsertSubscription = createMutation<SubscriptionsState>('upserting').with({
    name: 'subscriptions/upsert',
    mutateFn: (arg: { appId: string; userId: string; params: SubscribeDto }) => {
        return Clients.Users.postSubscriptions(arg.appId, arg.userId, { subscribe: [arg.params] });
    },
});

export const deleteSubscription = createMutation<SubscriptionsState>('upserting').with({
    name: 'subscriptions/delete',
    mutateFn: (arg: { appId: string; userId: string; topicPrefix: string }) => {
        return Clients.Users.postSubscriptions(arg.appId, arg.userId, { unsubscribe: [arg.topicPrefix] });
    },
});

export const subscriptionsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertSubscription.fulfilled.match(action) || deleteSubscription.fulfilled.match(action)) {
        const { appId, userId } = action.meta.arg;

        store.dispatch(loadSubscriptions({ appId, userId }) as any);
    } else if (deleteSubscription.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: SubscriptionsState = {
    subscriptions: loadSubscriptions.createInitial(),
};

const operations = [
    loadSubscriptions,
    upsertSubscription,
    deleteSubscription,
];

export const subscriptionsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }),
operations);
