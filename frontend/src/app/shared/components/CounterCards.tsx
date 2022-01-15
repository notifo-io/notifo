/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Card, CardBody, CardHeader, Col, Row, Table } from 'reactstrap';
import { texts } from '@app/texts';
import { Counters } from './../utils';
import { CHANNELS } from './../utils/model';
import { ChannelCounterRow } from './ChannelCounterRow';
import { StatisticsCard } from './StatisticsCard';

export interface CounterCardsProps {
    // The counters.
    counters: { [key: string]: number };
}

export const CounterCards = React.memo((props: CounterCardsProps) => {
    const {
        counters,
    } = props;

    return (
        <>
            <Row className='small-gutters'>
                <Col className='mb-4' xs={6}>
                    <StatisticsCard icon='message'
                        attempt={counters[Counters.NotificationsAttempt]}
                        failed={counters[Counters.NotificationsFailed]}
                        summary={counters[Counters.NotificationsHandled]}
                        summaryLabel={texts.common.created}
                        title={texts.common.notifications}
                    />
                </Col>
                <Col className='mb-4' xs={6}>
                    <Card className='statistics-card'>
                        <CardHeader>
                            {texts.common.channels}
                        </CardHeader>
                        <CardBody>
                            <Table borderless size='sm' className='table-sm-text'>
                                <tbody>
                                    {CHANNELS.map(channel =>
                                        <ChannelCounterRow key={channel} channel={channel} counters={counters} />,
                                    )}
                                </tbody>
                            </Table>
                        </CardBody>
                    </Card>
                </Col>
            </Row>
        </>
    );
});
