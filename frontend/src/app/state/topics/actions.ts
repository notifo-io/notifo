/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, TopicQueryScope, UpsertTopicDto } from '@app/service';
import { selectApp } from './../shared';
import { TopicsState, TopicsStateInStore } from './state';

export const loadTopics = createList<TopicsState, TopicsStateInStore>('topics', 'topics').with({
    name: 'topics/load',
    queryFn: async (p: { appId: string; scope: TopicQueryScope }, q) => {
        const { items, total } = await Clients.Topics.getTopics(p.appId, p.scope, q.search, q.take, q.skip);

        return { items, total };
    },
});

export const upsertTopic = createMutation<TopicsState>('upserting').with({
    name: 'topics/upsert',
    mutateFn: async (arg: { appId: string; scope: TopicQueryScope; params: UpsertTopicDto }) => {
        const response = await Clients.Topics.postTopics(arg.appId, { requests: [arg.params] });

        return response[0];
    },
});

export const deleteTopic = createMutation<TopicsState>('upserting').with({
    name: 'topics/delete',
    mutateFn: (arg: { appId: string; path: string; scope: TopicQueryScope }) => {
        return Clients.Topics.deleteTopic(arg.appId, arg.path);
    },
});

export const topicsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertTopic.fulfilled.match(action) || deleteTopic.fulfilled.match(action)) {
        const { appId, scope } = action.meta.arg;

        store.dispatch(loadTopics({ appId, scope }) as any);
    } else if (deleteTopic.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: TopicsState = {
    topics: loadTopics.createInitial(),
};

const operations = [
    deleteTopic,
    loadTopics,
    upsertTopic,
];

export const topicsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }),
operations);
