/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { FormatDate } from './FormatDate';

export interface TimelineItem {
    // The text items.
    text: string;

    // The date as string or number.
    date: number | string;
}

export interface TimelineProps {
    // The items to render.
    items?: ReadonlyArray<TimelineItem> | null;
}

export const Timeline = (props: TimelineProps) => {
    const { items } = props;

    if (!items || items.length === 0) {
        return null;
    }

    return (
        <div className='timeline'>
            <div className='timeline-v'></div>

            {items.map((item, i) =>
                <div className='timeline-item' key={i}>
                    <div className='timeline-date'>
                        <FormatDate date={item.date} />
                    </div>
                    <div className='timeline-text'>
                        {item.text}
                    </div>
                </div>,
            )}
        </div>
    );
};
