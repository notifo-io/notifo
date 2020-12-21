/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Counters } from '@app/shared/utils';
import { texts } from '@app/texts';
import * as React from 'react';
import { Col, Row } from 'reactstrap';
import { StatisticsLabel } from './StatisticsLabel';

export interface CounterRowProps {
    // The counters.
    counters: { [key: string]: number };

    // True to hide all counters.
    hideCounters?: boolean;

    // The children.
    children: React.ReactElement[];
}

export const CounterRow = React.memo((props: CounterRowProps) => {
    const { children, counters, hideCounters } = props;

    return (
        <>
            <tr className='list-item-summary'>
                {children}
            </tr>

            {!hideCounters &&
                <tr className='list-item-details'>
                    <td colSpan={children.length}>
                        <Row>
                            <Col xs={4}>
                                <StatisticsLabel icon='message'
                                    name={texts.common.notifications}
                                    total={counters[Counters.NotificationsHandled]}
                                    totalFailed={counters[Counters.NotificationsFailed]}
                                    totalAttempt={counters[Counters.NotificationsAttempt]} />
                            </Col>
                            <Col xs={2}>
                                <StatisticsLabel icon='browser'
                                    name={texts.common.webPush}
                                    total={counters[Counters.WebPushHandled]}
                                    totalFailed={counters[Counters.WebPushFailed]}
                                    totalAttempt={counters[Counters.WebPushAttempt]}
                                />
                            </Col>
                            <Col xs={2}>
                                <StatisticsLabel icon='mobile'
                                    name={texts.common.mobilePush}
                                    total={counters[Counters.MobilePushHandled]}
                                    totalFailed={counters[Counters.MobilePushFailed]}
                                    totalAttempt={counters[Counters.MobilePushAttempt]}
                                />
                            </Col>
                            <Col xs={2}>
                                <StatisticsLabel icon='mail_outline'
                                    name={texts.common.email}
                                    total={counters[Counters.EmailHandled]}
                                    totalFailed={counters[Counters.EmailFailed]}
                                    totalAttempt={counters[Counters.EmailAttempt]}
                                />
                            </Col>
                            <Col xs={2}>
                                <StatisticsLabel icon='sms'
                                    name={texts.common.sms}
                                    total={counters[Counters.SmsHandled]}
                                    totalFailed={counters[Counters.SmsFailed]}
                                    totalAttempt={counters[Counters.SmsAttempt]}
                                />
                            </Col>
                        </Row>
                    </td>
                </tr>
            }

            <tr className='list-item-separator'></tr>
        </>
    );
});
