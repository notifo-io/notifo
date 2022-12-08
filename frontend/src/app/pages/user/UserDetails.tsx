/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { NavLink } from 'react-router-dom';
import { Button, Col, FormGroup, Label, Row } from 'reactstrap';
import { ApiValue, FormatDate, Icon, useEventCallback } from '@app/framework';
import { UserDto } from '@app/service';
import { togglePublishDialog } from '@app/state';
import { texts } from '@app/texts';
import { ButtonEmail, ButtonMobilePush, ButtonSms, ButtonWebPush } from './Buttons';

export interface UserDetailsProps {
    // The user to show.
    user: UserDto;

    // The current app id.
    appId: string;
}

export const UserDetails = (props: UserDetailsProps) => {
    const { appId, user } = props;
    
    const dispatch = useDispatch();
    
    const doPublish = useEventCallback(() => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${user.id}` } }));
    });

    return (
        <>
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

            <div>
                <NavLink to={`/app/${appId}/log/${user.id}`}>
                    {texts.users.logs}
                </NavLink>
            </div>

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
        </>
    );
};