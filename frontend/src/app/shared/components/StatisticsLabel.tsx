/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Icon, IconType, Numbers, Types } from '@app/framework';
import { texts } from '@app/texts';

export interface StatisticsLabelProps {
    // The icon.
    icon: IconType;

    // The name of the statistics.
    name: string;

    // The total number of items.
    total?: number;

    // The failed items.
    totalFailed?: number;

    // The attempting items.
    totalAttempt?: number;
}

export const StatisticsLabel = React.memo((props: StatisticsLabelProps) => {
    const {
        name,
        icon,
        total,
        totalFailed,
        totalAttempt,
    } = props;

    return (
        <span className='statistics-label' data-tooltip-id="default-tooltip" data-tooltip-content={texts.common.statisticsLabelFn(name)}>
            <Icon className='statistics-label-icon' type={icon} />

            <span className='statistics-total'>
                {Numbers.formatNumber(total || 0)}
            </span>

            /

            {Types.isNumber(totalAttempt) ? (
                <span className='statistics-attempt'>
                    {Numbers.formatNumber(totalAttempt)}
                </span>
            ) : <span>-</span>}

            /

            {Types.isNumber(totalFailed) ? (
                <span className='statistics-failed'>
                    {Numbers.formatNumber(totalFailed)}
                </span>
            ) : <span>-</span>}
        </span>
    );
});
