/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { Middleware } from 'redux';
import { ErrorDto, listThunk, Query } from '@app/framework';
import { Clients, SubscribeDto, SubscriptionDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { SubscriptionsState } from './state';

const list = listThunk<SubscriptionsState, SubscriptionDto>('subscriptions', 'subscriptions', async (params) => {
    const { items, total } = await Clients.Users.getSubscriptions(params.appId, params.userId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadSubscriptions = (appId: string, userId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, userId, query, reset });
};

export const upsertSubscription = createApiThunk('subscriptions/upsert',
    (arg: { appId: string; userId: string; params: SubscribeDto }) => {
        return Clients.Users.postSubscriptions(arg.appId, arg.userId, { subscribe: [arg.params] });
    });

export const deleteSubscription = createApiThunk('subscriptions/delete',
    (arg: { appId: string; userId: string; topicPrefix: string }) => {
        return Clients.Users.postSubscriptions(arg.appId, arg.userId, { unsubscribe: [arg.topicPrefix] });
    });

export const subscriptionsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertSubscription.fulfilled.match(action) || deleteSubscription.fulfilled.match(action)) {
        const load: any = loadSubscriptions(action.meta.arg.appId, action.meta.arg.userId);

        store.dispatch(load);
    }

    return result;
};

const initialState: SubscriptionsState = {
    subscriptions: list.createInitial(),
};

export const subscriptionsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(upsertSubscription.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertSubscription.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertSubscription.fulfilled, (state) => {
        state.upserting = false;
        state.upsertingError = undefined;
    }));
