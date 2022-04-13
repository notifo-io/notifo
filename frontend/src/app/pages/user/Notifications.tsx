/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, Col, Nav, NavItem, NavLink, Row, Table } from 'reactstrap';
import { FormError, Icon, ListPager, ListSearch, Loader, Query } from '@app/framework';
import { CHANNELS } from '@app/shared/utils/model';
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
    const [channels, setChannels] = React.useState<string[]>([]);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadNotifications(appId, userId, {}, undefined, channels));
    }, [dispatch, appId, userId, channels]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadNotifications(appId, userId, undefined, undefined, channels));
    }, [dispatch, appId, userId, channels]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadNotifications(appId, userId, q, undefined, channels));
    }, [dispatch, appId, userId, channels]);

    const doToggleChannel = React.useCallback((channel: string) => {
        setChannels(channels => {
            let newChannels: string[];

            if (channels.length === 0) {
                newChannels = CHANNELS.filter(x => x !== channel);
            } else if (channels.indexOf(channel) >= 0) {
                newChannels = channels.filter(x => x !== channel);
            } else {
                newChannels = [...channels, channel];
            }

            return newChannels.length >= CHANNELS.length ? [] : newChannels;
        });
    }, []);

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs={12} lg={6}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col className='col-button'>
                            <Nav className='nav-tabs2'>
                                <NavItem>
                                    <NavLink active>
                                        {texts.topics.header}
                                    </NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink onClick={onSwitch}>
                                        {texts.topics.explicit}
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

            <Row className='channels-filter' noGutters>
                {CHANNELS.map(channel => 
                    <Col xs={2} key={channel}>
                        <Button block color='blank' className={classNames('btn-flat', { active: channels.indexOf(channel) >= 0 || channels.length === 0 })} onClick={() => doToggleChannel(channel)}>
                            {texts.notificationSettings[channel].name}
                        </Button>
                    </Col>,
                )}
            </Row>

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
