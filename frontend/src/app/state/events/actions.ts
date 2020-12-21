/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { List, Query } from '@app/framework';
import { Clients, EventDto } from '@app/service';
import { Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { EventsState } from './state';

const list = new List<EventDto>('events', 'events', async (params) => {
    const { items, total } = await Clients.Events.getEvents(params.appId, params.search, params.pageSize, params.page * params.pageSize);

    return { items, total };
});

export const loadEventsAsync = (appId: string, q?: Partial<Query>, reset = false) => {
    return list.load(q, { appId }, reset);
};

export function eventsReducer(): Reducer<EventsState> {
    const initialState: EventsState = {
        events: list.createInitial(),
    };

    const reducer: Reducer<EventsState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
