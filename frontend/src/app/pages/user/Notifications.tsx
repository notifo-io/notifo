/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, Col, Nav, NavItem, NavLink, Row, Table } from 'reactstrap';
import { FormError, Icon, ListMultiFilter, ListPager, ListSearch, Loader, Query, useEventCallback } from '@app/framework';
import { CHANNELS } from '@app/shared/utils/model';
import { loadNotifications, useApp, useNotifications } from '@app/state';
import { texts } from '@app/texts';
import { NotificationRow } from './NotificationRow';

const NON_WEBHOOKS = CHANNELS.filter(x => x !== 'webhook').map(value => ({
    value,
    label: texts.notificationSettings[value]?.name || value,
}));

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
    const [channels, setChannels] = React.useState<string[]>([]);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadNotifications(appId, userId, {}, undefined, channels));
    }, [dispatch, appId, userId, channels]);

    const doRefresh = useEventCallback(() => {
        dispatch(loadNotifications(appId, userId, undefined, undefined, channels));
    });

    const doLoad = useEventCallback((q?: Partial<Query>) => {
        dispatch(loadNotifications(appId, userId, q, undefined, channels));
    });

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs={12} lg={6}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col className='col-button'>
                            <Nav className='nav-tabs2'>
                                <NavItem>
                                    <NavLink active>
                                        {texts.notifications.header}
                                    </NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink onClick={onSwitch}>
                                        {texts.subscriptions.header}
                                    </NavLink>
                                </NavItem>
                            </Nav>
                        </Col>
                        <Col xs='auto' className='col-refresh'>
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

            <ListMultiFilter value={channels} onChange={setChannels} options={NON_WEBHOOKS} />

            <Card className='card-table'>
                <CardBody>
                    <Table className='table-fixed table-simple table-middle'>
                        <colgroup>
                            <col style={{ width: 50 }} />
                            <col />
                            <col style={{ width: 25 }}  />
                            <col style={{ width: 25 }}  />
                            <col style={{ width: 25 }}  />
                            <col style={{ width: 25 }}  />
                            <col style={{ width: 240 }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>&nbsp;</th>
                                <th>
                                    <span className='truncate'>{texts.common.subject}</span>
                                </th>
                                <th colSpan={4} data-tip={`${texts.common.handled} / ${texts.common.delivered} / ${texts.common.seen} / ${texts.common.confirmed}`}>
                                    <span className='truncate'>{texts.common.status}</span>
                                </th>
                                <th className='text-right'>
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

                            {notifications.isLoaded && notifications.items && notifications.items.length === 0 &&
                                <tr>
                                    <td colSpan={7}>{texts.notifications.notificationsNotFound}</td>
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
