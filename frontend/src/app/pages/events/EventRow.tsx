/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FromNow } from '@app/framework';
import { EventDto } from '@app/service';
import { CounterRow } from '@app/shared/components';
import * as React from 'react';

export interface EventRowProps {
    // The event.
    event: EventDto;

    // True to hide all counters.
    hideCounters?: boolean;
}

export const EventRow = React.memo((props: EventRowProps) => {
    const { event, hideCounters } = props;

    return (
        <CounterRow counters={event.counters} hideCounters={hideCounters}>
            <td>
                <span className='truncate'>{event.displayName}</span>
            </td>
            <td>
                <span className='truncate mono'>{event.topic}</span>
            </td>
            <td>
                <span className='truncate'>
                    <FromNow date={event.created} />
                </span>
            </td>
        </CounterRow>
    );
});
