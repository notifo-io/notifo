/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon, IconType, Numbers, Types } from '@app/framework';
import { texts } from '@app/texts';
import * as React from 'react';
import { Card, CardBody, CardHeader, Col, Row } from 'reactstrap';

export interface StatisticsCardProps {
    icon: IconType;

    // The total number of items.
    summary?: number;

    // The total label.
    summaryLabel?: string;

    // The failed items.
    failed?: number;

    // The attempt items.
    attempt?: number;

    // The optional title.
    title?: string;
}

export const StatisticsCard = React.memo((props: StatisticsCardProps) => {
    const {
        failed,
        icon,
        summary,
        summaryLabel,
        title,
        attempt,
    } = props;

    return (
        <Card className='statistics-card'>
            {title &&
                <CardHeader>
                    <Icon className='statistics-card-icon' type={icon} /> {title}
                </CardHeader>
            }
            <CardBody>
                <Row style={{ flexWrap: 'nowrap' }} noGutters>
                    <Col>
                        <div className='statistics-card-total statistics-total'>
                            {Numbers.formatNumber(summary || 0)}

                            {summaryLabel &&
                                <small>{summaryLabel}</small>
                            }
                        </div>
                    </Col>
                </Row>
                <Row>
                    <Col className='statistics-card-detail'>
                        {Types.isNumber(failed) &&
                            <>
                                <div className='statistics-failed'>
                                    <Icon type='error_outline' /> {Numbers.formatNumber(failed)}
                                </div>

                                <small>{texts.common.failed}</small>
                            </>
                        }
                    </Col>

                    <Col className='statistics-card-detail'>
                        {Types.isNumber(attempt) &&
                            <>
                                <div className='statistics-attempt'>
                                    <Icon type='hourglass_empty' /> {Numbers.formatNumber(attempt)}
                                </div>

                                <small>{texts.common.attempt}</small>
                            </>
                        }
                    </Col>
                </Row>
            </CardBody>
        </Card>
    );
});
