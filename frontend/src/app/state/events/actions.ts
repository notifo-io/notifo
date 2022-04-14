/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { listThunk, Query } from '@app/framework';
import { Clients, EventDto } from '@app/service';
import { selectApp } from '../shared';
import { EventsState } from './state';

const list = listThunk<EventsState, EventDto>('events', 'events', async params => {
    const { items, total } = await Clients.Events.getEvents(params.appId, params.channels, params.search, params.take, params.skip);

    return { items, total };
});

export const loadEvents = (appId: string, query?: Partial<Query>, reset = false, channels?: string[]) => {
    return list.action({ appId, query, reset, channels });
};

const initialState: EventsState = {
    events: list.createInitial(),
};

export const eventsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    }));
