/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, ButtonGroup, Card, CardBody, Col, Row, Table } from 'reactstrap';
import { FormError, Icon, ListPager, ListSearch, Loader, Query } from '@app/framework';
import { loadNotifications, useApp, useNotifications } from '@app/state';
import { texts } from '@app/texts';
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
    const app = useApp()!;
    const appId = app.id;
    const notifications = useNotifications(x => x.notifications);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadNotifications(appId, userId, {}));
    }, [dispatch, appId, userId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadNotifications(appId, userId));
    }, [dispatch, appId, userId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadNotifications(appId, userId, q));
    }, [dispatch, appId, userId]);

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs={12} lg={6}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col className='col-button'>
                            <ButtonGroup>
                                <Button Button color='simple' className='btn-flat truncate active'>
                                    {texts.notifications.header}
                                </Button>
                                <Button Button color='simple' className='btn-flat truncate' outline onClick={onSwitch}>
                                    {texts.subscriptions.header}
                                </Button>
                            </ButtonGroup>
                        </Col>
                        <Col xs='auto'>
                            {notifications.isLoading ? (
                                <Loader visible={notifications.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' className='btn-flat' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} lg={6}>
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
                            <col style={{ width: 200 }} />
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
