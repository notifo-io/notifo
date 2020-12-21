/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { List, Query } from '@app/framework';
import { Clients, LogEntryDto } from '@app/service';
import { Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { LogState } from './state';

export const LOG_LOAD_STARTED = 'LOG_LOAD_STARTED';
export const LOG_LOAD_FAILED = 'LOG_LOAD_FAILED';
export const LOG_LOAD_SUCCEEEDED = 'LOG_LOAD_SUCCEEEDED';

const list = new List<LogEntryDto>('log', 'logEntries', async (params) => {
    const { items, total } = await Clients.Logs.getLogs(params.appId, params.search, params.pageSize, params.page * params.pageSize);

    return { items, total };
});

export const loadLogAsync = (appId: string, q?: Partial<Query>, reset = false) => {
    return list.load(q, { appId }, reset);
};

export function logReducer(): Reducer<LogState> {
    const initialState: LogState = {
        logEntries: list.createInitial(),
    };

    const reducer: Reducer<LogState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
