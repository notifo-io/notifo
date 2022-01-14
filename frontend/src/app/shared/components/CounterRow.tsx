/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Col, Row, Table } from 'reactstrap';
import { Counters } from '@app/shared/utils';
import { texts } from '@app/texts';
import { CHANNELS } from './../utils/model';
import { ChannelCounterRow } from './ChannelCounterRow';
import { StatisticsLabel } from './StatisticsLabel';

export interface CounterRowProps {
    // The counters.
    counters: { [key: string]: number };

    // True to show all counters.
    showCounters?: boolean;

    // The number of columns.
    columnCount: number;

    // The children.
    children: React.ReactElement[] | React.ReactElement | null;
}

export const CounterRow = React.memo((props: CounterRowProps) => {
    const { children, columnCount, counters, showCounters } = props;

    return (
        <>
            {children}

            {showCounters &&
                <tr className='list-item-details'>
                    <td colSpan={columnCount}>
                        <Row>
                            <Col xs={6}>
                                <StatisticsLabel icon='message'
                                    name={texts.common.notifications}
                                    total={counters[Counters.NotificationsHandled]}
                                    totalFailed={counters[Counters.NotificationsFailed]}
                                    totalAttempt={counters[Counters.NotificationsAttempt]} />
                            </Col>
                            <Col x={6}>
                                <Table borderless size='sm' className='table-sm-text'>
                                    <tbody>
                                        {CHANNELS.map(channel =>
                                            <ChannelCounterRow key={channel} channel={channel} counters={counters} />,
                                        )}
                                    </tbody>
                                </Table>
                            </Col>
                        </Row>
                    </td>
                </tr>
            }

            <tr className='list-item-separator'></tr>
        </>
    );
});
