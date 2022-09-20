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
import { ApiValue, FormatDate, Icon, Loader, useBooleanObj, useEventCallback } from '@app/framework';
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
    const dialogEdit = useBooleanObj();
    const loading = useUsers(x => x.loadingUser);
    const match = useRouteMatch();
    const user = useUsers(x => x.user)!;
    const userId = match.params['userId'];
    const [activeTab, setActiveTab] = React.useState('notifications');

    React.useEffect(() => {
        dispatch(loadUser({ appId, userId }));
    }, [dispatch, appId, userId]);

    const doPublish = useEventCallback(() => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${userId}` } }));
    });

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
                                            <a className='btn btn-secondary-link' target='_blank' href={`/demo.html?userToken=${user.apiKey}`}>
                                                <Icon type='code' /> {texts.common.demo}
                                            </a>
                                        </Col>
                                    </Row>

                                    <hr />

                                    <Row>
                                        <Col xs={3}>
                                            <ButtonWebPush appId={appId} user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonMobilePush appId={appId} user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonEmail user={user} />
                                        </Col>
                                        <Col xs={3}>
                                            <ButtonSms user={user} />
                                        </Col>
                                    </Row>

                                    <hr />

                                    <FormGroup row>
                                        <Col xs={5}>
                                            <Label className='truncate'>{texts.common.created}</Label>
                                        </Col>
                                        <Col xs={7} className='text-right text-sm'>
                                            <FormatDate date={user.created} />
                                        </Col>
                                    </FormGroup>

                                    <FormGroup row>
                                        <Col xs={5}>
                                            <Label className='truncate'>{texts.common.lastUpdate}</Label>
                                        </Col>
                                        <Col xs={7} className='text-right text-sm'>
                                            <FormatDate date={user.lastUpdate} />
                                        </Col>
                                    </FormGroup>

                                    <FormGroup row>
                                        <Col xs={5}>
                                            <Label className='truncate'>{texts.common.lastNotification}</Label>
                                        </Col>
                                        <Col xs={7} className='text-right text-sm'>
                                            <FormatDate date={user.lastNotification} />
                                        </Col>
                                    </FormGroup>

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
