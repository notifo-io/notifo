/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Code } from '@app/framework';
import { EventDto } from '@app/service';

export interface EventDetailsProps {
    // The event.
    event: EventDto;

    // True to show all counters.
    showCounters?: boolean;
}

export const EventDetails = React.memo((props: EventDetailsProps) => {
    const { event } = props;

    return (
        <Code value={event} />
    );
});
