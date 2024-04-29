/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createExtendedReducer, createList } from '@app/framework';
import { Clients } from '@app/service';
import { selectApp } from './../shared';
import { LogState, LogStateInStore } from './state';

export const loadLog = createList<LogState, LogStateInStore>('log', 'log').with({
    name: 'log/load',
    queryFn: async (p: { appId: string; systems?: string[]; userId?: string }, q ) => {
        const { items, total } = await Clients.Logs.getLogs(p.appId, p.systems, p.userId, undefined, q.search, q.take, q.skip);

        return { items, total };
    },
});

const initialState: LogState = {
    log: loadLog.createInitial(),
};

const operations = [
    loadLog,
];

export const logReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }), operations);
