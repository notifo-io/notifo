/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { listThunk, Query } from '@app/framework';
import { Clients, EventDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { selectApp } from '../shared';
import { EventsState } from './state';

const list = listThunk<EventsState, EventDto>('events', 'events', async params => {
    const { items, total } = await Clients.Events.getEvents(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadEventsAsync = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

const initialState: EventsState = {
    events: list.createInitial(),
};

export const eventsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
