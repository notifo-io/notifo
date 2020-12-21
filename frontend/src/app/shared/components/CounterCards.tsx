/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';
import * as React from 'react';
import { Col, Row } from 'reactstrap';
import { Counters } from './../utils';
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
                <Col className='mb-4' lg={3} xs={12}>
                    <StatisticsCard icon='message'
                        attempt={counters[Counters.NotificationsAttempt]}
                        summary={counters[Counters.NotificationsHandled]}
                        summaryLabel={texts.common.created}
                        title={texts.common.notifications}
                    />
                </Col>
                <Col lg={9} xs={12}>
                    <Row className='small-gutters'>
                        <Col className='mb-4' lg={3} xs={6}>
                            <StatisticsCard icon='browser'
                                failed={counters[Counters.WebPushFailed]}
                                attempt={counters[Counters.WebPushAttempt]}
                                summary={counters[Counters.WebPushHandled]}
                                summaryLabel={texts.common.sent}
                                title={texts.common.webPush}
                            />
                        </Col>
                        <Col className='mb-4' lg={3} xs={6}>
                            <StatisticsCard icon='mobile'
                                failed={counters[Counters.MobilePushFailed]}
                                attempt={counters[Counters.MobilePushAttempt]}
                                summary={counters[Counters.MobilePushHandled]}
                                summaryLabel={texts.common.sent}
                                title={texts.common.mobilePush}
                            />
                        </Col>
                        <Col className='mb-4' lg={3} xs={6}>
                            <StatisticsCard icon='mail_outline'
                                failed={counters[Counters.EmailFailed]}
                                attempt={counters[Counters.EmailAttempt]}
                                summary={counters[Counters.EmailHandled]}
                                summaryLabel={texts.common.sent}
                                title={texts.common.emails}
                            />
                        </Col>
                        <Col className='mb-4' lg={3} xs={6}>
                            <StatisticsCard icon='sms'
                                failed={counters[Counters.SmsFailed]}
                                attempt={counters[Counters.SmsAttempt]}
                                summary={counters[Counters.SmsHandled]}
                                summaryLabel={texts.common.sent}
                                title={texts.common.sms}
                            />
                        </Col>
                    </Row>
                </Col>
            </Row>
        </>
    );
});
