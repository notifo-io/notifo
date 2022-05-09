/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer, Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { ErrorInfo, formatError, listThunk, Query } from '@app/framework';
import { Clients, TopicDto, TopicQueryScope, UpsertTopicDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { TopicsState } from './state';

const list = listThunk<TopicsState, TopicDto>('topics', 'topics', async params => {
    const { items, total } = await Clients.Topics.getTopics(params.appId, params.scope, params.search, params.take, params.skip);

    return { items, total };
});

export const loadTopics = (appId: string, scope: TopicQueryScope, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, scope, query, reset });
};

export const upsertTopic = createApiThunk('topics/upsert',
    async (arg: { appId: string; scope: TopicQueryScope; params: UpsertTopicDto }) => {
        const response = await Clients.Topics.postTopics(arg.appId, { requests: [arg.params] });

        return response[0];
    });

export const deleteTopic = createApiThunk('topics/delete',
    (arg: { appId: string; path: string; scope: TopicQueryScope }) => {
        return Clients.Topics.deleteTopic(arg.appId, arg.path);
    });

export const topicsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertTopic.fulfilled.match(action) || deleteTopic.fulfilled.match(action)) {
        const { appId, scope } = action.meta.arg;

        store.dispatch(loadTopics(appId, scope) as any);
    } else if (deleteTopic.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: TopicsState = {
    topics: list.createInitial(),
};

export const topicsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(upsertTopic.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertTopic.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorInfo;
    })
    .addCase(upsertTopic.fulfilled, (state) => {
        state.upserting = false;
        state.upsertingError = undefined;
    }));
