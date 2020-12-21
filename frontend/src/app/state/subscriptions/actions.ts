/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError, List, Query } from '@app/framework';
import { Clients, SubscribeDto, SubscriptionDto } from '@app/service';
import { Dispatch, Middleware, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { SubscriptionsState } from './state';

export const SUBSCRIPTION_UPSERT_STARTED = 'SUBSCRIPTION_UPSERT_STARTED';
export const SUBSCRIPTION_UPSERT_FAILED = 'SUBSCRIPTION_UPSERT_FAILED';
export const SUBSCRIPTION_UPSERT_SUCCEEEDED = 'SUBSCRIPTION_UPSERT_SUCCEEEDED';
export const SUBSCRIPTION_DELETE_STARTED = 'SUBSCRIPTION_DELETE_STARTED';
export const SUBSCRIPTION_DELETE_FAILED = 'SUBSCRIPTION_DELETE_FAILED';
export const SUBSCRIPTION_DELETE_SUCCEEEDED = 'SUBSCRIPTION_DELETE_SUCCEEEDED';

const list = new List<SubscriptionDto>('subscriptions', 'subscriptions', async (params) => {
    const { items, total } = await Clients.Users.getSubscriptions(params.appId, params.userId, params.search, params.pageSize, params.page * params.pageSize);

    return { items, total };
});

export const loadSubscriptionsAsync = (appId: string, userId: string, q?: Partial<Query>, reset = false) => {
    return list.load(q, { appId, userId }, reset);
};

export const upsertSubscriptionAsync = (appId: string, userId: string, params: SubscribeDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: SUBSCRIPTION_UPSERT_STARTED });

        try {
            await Clients.Users.postSubscription(appId, userId, params);

            dispatch({ type: SUBSCRIPTION_UPSERT_SUCCEEEDED, appId, userId });

        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: SUBSCRIPTION_UPSERT_FAILED, error });
        }
    };
};

export const deleteSubscriptionAsync = (appId: string, userId: string, prefix: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: SUBSCRIPTION_DELETE_FAILED });

        try {
            await Clients.Users.deleteSubscription(appId, userId, prefix);

            dispatch({ type: SUBSCRIPTION_DELETE_SUCCEEEDED, appId });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: SUBSCRIPTION_DELETE_FAILED, error });
        }
    };
};

export function subscriptionsMiddleware(): Middleware {
    const middleware: Middleware = store => next => action => {
        const result = next(action);

        if (action.type === SUBSCRIPTION_UPSERT_SUCCEEEDED || action.type === SUBSCRIPTION_DELETE_SUCCEEEDED) {
            const load: any = loadSubscriptionsAsync(action.appId, action.userId);

            store.dispatch(load);
        }

        return result;
    };

    return middleware;
}

export function subscriptionsReducer(): Reducer<SubscriptionsState> {
    const initialState: SubscriptionsState = {
        subscriptions: list.createInitial(),
    };

    const reducer: Reducer<SubscriptionsState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            case SUBSCRIPTION_UPSERT_STARTED:
                return {
                    ...state,
                    upserting: true,
                    upsertingError: null,
                };
            case SUBSCRIPTION_UPSERT_FAILED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: action.error,
                };
            case SUBSCRIPTION_UPSERT_SUCCEEEDED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: null,
                };
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
