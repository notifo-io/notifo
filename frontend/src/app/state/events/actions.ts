/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createExtendedReducer, createList } from '@app/framework';
import { Clients } from '@app/service';
import { selectApp } from './../shared';
import { EventsState, EventsStateInStore } from './state';

export const loadEvents = createList<EventsState, EventsStateInStore>('events', 'events').with({
    name: 'events/load',
    queryFn: async (p: { appId: string; channels?: string[] }, q) => {
        const { items, total } = await Clients.Events.getEvents(p.appId, p.channels, q.search, q.take, q.skip);

        return { items, total };
    },
});

const initialState: EventsState = {
    events: loadEvents.createInitial(),
};

const operations = [
    loadEvents,
];

export const eventsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }), operations);
