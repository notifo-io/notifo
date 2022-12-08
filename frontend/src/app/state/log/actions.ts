/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { listThunk, Query } from '@app/framework';
import { Clients, LogEntryDto } from '@app/service';
import { selectApp } from './../shared';
import { LogState } from './state';

const list = listThunk<LogState, LogEntryDto>('log', 'entries', async params => {
    const { items, total } = await Clients.Logs.getLogs(params.appId, params.systems, params.userId, undefined, params.search, params.take, params.skip);

    return { items, total };
});

export const loadLog = (appId: string, query?: Partial<Query>, reset = false, systems?: string[], userId?: string) => {
    return list.action({ appId, query, reset, systems, userId });
};

const initialState: LogState = {
    entries: list.createInitial(),
};

export const logReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
