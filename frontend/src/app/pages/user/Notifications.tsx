/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListPager, ListSearch, Loader, Query } from '@app/framework';
import { getApp, loadNotificationsAsync, useApps, useNotifications } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, ButtonGroup, Card, CardBody, Col, Row, Table } from 'reactstrap';
import { NotificationRow } from './NotificationRow';

export interface NotificationsProps {
    // The user id.
    userId: string;

    // Toggled when switched to other tab.
    onSwitch?: () => void;
}

export const Notifications = (props: NotificationsProps) => {
    const { onSwitch, userId } = props;

    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const notifications = useNotifications(x => x.notifications);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadNotificationsAsync(appId, userId, {}));
    }, [appId, userId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadNotificationsAsync(appId, userId));
    }, [appId, userId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadNotificationsAsync(appId, userId, q));
    }, [appId, userId]);

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs={12} lg={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <ButtonGroup>
                                <Button Button color='simple' className='btn-flat active'>
                                    {texts.notifications.header}
                                </Button>
                                <Button Button color='simple' className='btn-flat' outline onClick={onSwitch}>
                                    {texts.subscriptions.header}
                                </Button>
                            </ButtonGroup>
                        </Col>
                        <Col xs='auto'>
                            {notifications.isLoading ? (
                                <Loader visible={notifications.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} lg={7}>
                    <ListSearch list={notifications} onSearch={doLoad} placeholder={texts.notifications.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={notifications.error} />

            <Card className='card-table'>
                <CardBody>
                    <Table className='table-fixed table-simple table-middle'>
                        <colgroup>
                            <col style={{ width: 50 }} />
                            <col />
                            <col style={{ width: 170 }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>&nbsp;</th>
                                <th>
                                    <span className='truncate'>{texts.common.subject}</span>
                                </th>
                                <th>
                                    <span className='truncate'>{texts.common.timestamp}</span>
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {notifications.items &&
                                <>
                                    {notifications.items.map(notification => (
                                        <NotificationRow key={notification.id} notification={notification} />
                                    ))}
                                </>
                            }

                            {!notifications.isLoading && notifications.items && notifications.items.length === 0 &&
                                <tr>
                                    <td colSpan={4}>{texts.notifications.notificationsNotFound}</td>
                                </tr>
                            }
                        </tbody>
                    </Table>
                </CardBody>
            </Card>

            <ListPager list={notifications} onChange={doLoad} />
        </>
    );
};
