/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { listThunk, Query } from '@app/framework';
import { Clients, LogEntryDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { selectApp } from './../shared';
import { LogState } from './state';

const list = listThunk<LogState, LogEntryDto>('events', 'events', async params => {
    const { items, total } = await Clients.Logs.getLogs(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadLogAsync = (appId: string, q?: Partial<Query>, reset = false) => {
    return list.action({ query: q, params: { appId }, reset });
};

const initialState: LogState = {
    logEntries: list.createInitial(),
};

export const logReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
