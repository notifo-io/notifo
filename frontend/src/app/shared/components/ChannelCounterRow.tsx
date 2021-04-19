/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon, Numbers, Types } from '@app/framework';
import { texts } from '@app/texts';
import * as React from 'react';

export interface ChannelCounterRowProps {
    // The name of the channel.
    channel: string;

    // The counters.
    counters: { [key: string]: number };
}

export const ChannelCounterRow = (props: ChannelCounterRowProps) => {
    const {
        channel,
        counters,
    } = props;

    const counterHandled = counters[`${channel}_handled`];
    const counterAttempt = counters[`${channel}_failed`];
    const counterFailed = counters[`${channel}_failed`];

    return (
        <tr key={channel}>
            <td>
                <Icon type={channel as any} /> {texts.notificationSettings[channel].name}
            </td>
            <td>
                {Numbers.formatNumber(counterHandled || 0)}
            </td>
            <td>
                {Types.isNumber(counterFailed) &&
                    <div className='statistics-failed'>
                        <Icon type='error_outline' /> {Numbers.formatNumber(counterFailed)}
                    </div>
                }
            </td>
            <td>
                {Types.isNumber(counterAttempt) &&
                    <div className='statistics-attempt'>
                        <Icon type='hourglass_empty' /> {Numbers.formatNumber(counterAttempt)}
                    </div>
                }
            </td>
        </tr>
    );
};
