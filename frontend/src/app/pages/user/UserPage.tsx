/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { Button, Card, CardBody, Col, FormGroup, Label, Row } from 'reactstrap';
import { ApiValue, Icon, Loader, useDialog } from '@app/framework';
import { CounterCards } from '@app/shared/components';
import { loadUser, togglePublishDialog, useApp, useUsers } from '@app/state';
import { texts } from '@app/texts';
import { UserDialog } from './../users/UserDialog';
import { ButtonEmail, ButtonMobilePush, ButtonSms, ButtonWebPush } from './Buttons';
import { Notifications } from './Notifications';
import { Subscriptions } from './Subscriptions';

export const UserPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const [activeTab, setActiveTab] = React.useState('notifications');
    const loading = useUsers(x => x.loadingUser);
    const match = useRouteMatch();
    const user = useUsers(x => x.user)!;
    const userId = match.params['userId'];
    const dialogEdit = useDialog();

    React.useEffect(() => {
        dispatch(loadUser({ appId, userId }));
    }, [dispatch, appId, userId]);

    const doDemo = React.useCallback(() => {
        window.open(`/demo.html?userToken=${user.apiKey}`);
    }, [user]);

    const doPublish = React.useCallback(() => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${userId}` } }));
    }, [dispatch, userId]);

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
                                    <Row>
                                        <Col>
                                            <Button color='info' onClick={doPublish}>
                                                <Icon type='send' /> {texts.common.publish}
                                            </Button>
                                        </Col>

                                        <Col xs='auto'>
                                            <Button color='secondary-link' onClick={doDemo}>
                                                <Icon type='code' /> {texts.common.demo}
                                            </Button>
                                        </Col>
                                    </Row>

                                    <hr />

                                    <Row>
                                        <Col xs={3}>
                                            <ButtonWebPush user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonMobilePush user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonEmail user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonSms user={user} />
                                        </Col>
                                    </Row>

                                    <hr />

                                    <FormGroup>
                                        <Label>{texts.common.id}</Label>

                                        <ApiValue value={user.id} />
                                    </FormGroup>

                                    <FormGroup>
                                        <Label>{texts.app.apiKey}</Label>

                                        <ApiValue value={user.apiKey} />
                                    </FormGroup>

                                    <FormGroup>
                                        <Label>{texts.common.emailAddress}</Label>

                                        <ApiValue value={user.emailAddress} />
                                    </FormGroup>

                                    <Button color='primary' onClick={dialogEdit.open}>
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

            {dialogEdit.isOpen &&
                <UserDialog user={user} onClose={dialogEdit.close} />
            }
        </div>
    );
};
