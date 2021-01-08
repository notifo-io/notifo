/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, listThunk, Query } from '@app/framework';
import { Clients, SubscriptionDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { Middleware } from 'redux';
import { createApiThunk, selectApp } from './../shared';
import { SubscriptionsState } from './state';

const list = listThunk<SubscriptionsState, SubscriptionDto>('subscriptions', 'subscriptions', async (params) => {
    const { items, total } = await Clients.Users.getSubscriptions(params.appId, params.userId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadSubscriptionsAsync = (appId: string, userId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, userId, query, reset });
};

export const upsertSubscriptionAsync = createApiThunk('subscriptions/upsert',
    (arg: { appId: string, userId: string, params: SubscriptionDto }) => {
        return Clients.Users.postSubscription(arg.appId, arg.userId, arg.params);
    });

export const deleteSubscriptionAsync = createApiThunk('subscriptions/delete',
    (arg: { appId: string, userId: string, prefix: string }) => {
        return Clients.Users.deleteSubscription(arg.appId, arg.userId, arg.prefix);
    });

export const subscriptionsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertSubscriptionAsync.fulfilled.match(action) || deleteSubscriptionAsync.fulfilled.match(action)) {
        const load: any = loadSubscriptionsAsync(action.meta.arg.appId, action.meta.arg.userId);

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
    .addCase(upsertSubscriptionAsync.pending, (state) => {
        state.upserting = true,
        state.upsertingError = undefined;
    })
    .addCase(upsertSubscriptionAsync.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertSubscriptionAsync.fulfilled, (state) => {
        state.upserting = false;
        state.upsertingError = undefined;
    }));
