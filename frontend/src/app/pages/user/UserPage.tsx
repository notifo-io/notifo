/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { Button, Card, CardBody, Col, Row } from 'reactstrap';
import { Icon, Loader, useBooleanObj } from '@app/framework';
import { CounterCards } from '@app/shared/components';
import { loadUser, useApp, useUsers } from '@app/state';
import { texts } from '@app/texts';
import { UserDialog } from './../users/UserDialog';
import { Notifications } from './Notifications';
import { Subscriptions } from './Subscriptions';
import { UserDetails } from './UserDetails';

export const UserPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const dialogEdit = useBooleanObj();
    const loading = useUsers(x => x.loadingUser);
    const match = useRouteMatch();
    const user = useUsers(x => x.user)!;
    const userId = match.params['userId'];
    const [activeTab, setActiveTab] = React.useState('notifications');

    React.useEffect(() => {
        dispatch(loadUser({ appId, userId }));
    }, [dispatch, appId, userId]);

    return (
        <div className='user'>
            <Row className='align-items-center header'>
                <Col xs='auto'>
                    <h2>{texts.users.userDetails}</h2>
                </Col>
                <Col>
                    <Loader visible={loading} />
                </Col>
            </Row>

            {user ? (
                <>
                    <CounterCards counters={user.counters} />

                    <Row>
                        <Col className='mb-4'>
                            {activeTab === 'subscriptions' ? (
                                <Subscriptions userId={userId} onSwitch={() => setActiveTab('notifications')} />
                            ) : (
                                <Notifications userId={userId} onSwitch={() => setActiveTab('subscriptions')} />
                            )}
                        </Col>

                        <Col xs='auto' className='user-info'>
                            <Card>
                                <CardBody>
                                    <UserDetails appId={appId} user={user} />

                                    <Button color='primary' onClick={dialogEdit.on}>
                                        <Icon type='create' /> {texts.common.edit}
                                    </Button>
                                </CardBody>
                            </Card>
                        </Col>
                    </Row>
                </>
            ) : !loading ? (
                <>{texts.users.userNotFound}</>
            ) : null}

            {dialogEdit.value &&
                <UserDialog user={user} onClose={dialogEdit.off} />
            }
        </div>
    );
};
