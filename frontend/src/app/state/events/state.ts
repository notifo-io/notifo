/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState } from '@app/framework';
import { EventDto } from '@app/service';

export interface EventsStateInStore {
    events: EventsState;
}

export interface EventsState {
    // All events.
    events: ListState<EventDto>;
}
