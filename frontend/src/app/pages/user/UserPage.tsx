/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ApiValue, Icon, Loader, useDialog } from '@app/framework';
import { CounterCards } from '@app/shared/components';
import { getApp, loadUserAsync, togglePublishDialog, useApps, useUsers } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { Button, Card, CardBody, Col, FormGroup, Label, Row } from 'reactstrap';
import { UserDialog } from './../users/UserDialog';
import { Subscriptions } from './Subscriptions';

export const UserPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const loading = useUsers(x => x.loadingUser);
    const match = useRouteMatch();
    const user = useUsers(x => x.user)!;
    const userId = match.params['userId'];
    const dialogEdit = useDialog();

    React.useEffect(() => {
        dispatch(loadUserAsync({ appId, userId }));
    }, [appId, userId]);

    const doEdit = React.useCallback(() => {
        dialogEdit.open();
    }, []);

    const doDemo = React.useCallback(() => {
        window.open(`/demo.html?${user.apiKey}`);
    }, [user]);

    const doPublish = React.useCallback(() => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${userId}` } }));
    }, [userId]);

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
                            <Subscriptions userId={userId} />
                        </Col>
                        <Col xs='auto' className='user-info'>
                            <Card>
                                <CardBody>
                                    <FormGroup>
                                        <Button color='info' onClick={doPublish}>
                                            <Icon type='send' /> {texts.common.publish}
                                        </Button>

                                        <Button color='secondary-link' onClick={doDemo}>
                                            <Icon type='code' /> {texts.common.demo}
                                        </Button>
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

                                    <Button color='primary' onClick={doEdit}>
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
