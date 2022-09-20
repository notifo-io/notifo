/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, CardFooter, Col, Input, Row } from 'reactstrap';
import { FormError, Icon, useEventCallback } from '@app/framework';
import { AppDetailsDto } from '@app/service';
import { addContributor, removeContributor, useApps, useLogin } from '@app/state';
import { texts } from '@app/texts';

export interface ContributorsProps {
    // The app details.
    appDetails: AppDetailsDto;
}

export const Contributors = (props: ContributorsProps) => {
    const { appDetails } = props;

    const dispatch = useDispatch();
    const [email, setEmail] = React.useState('');
    const contributorsError = useApps(x => x.contributorsError);
    const contributorsUpdating = useApps(x => x.contributorsUpdating);
    const userId = useLogin(x => x.user?.sub);

    const doSetEmail = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setEmail(event.target.value);
    });

    const doInvite = useEventCallback(() => {
        dispatch(addContributor({ appId: appDetails.id, params: { email, role: 'Admin' } }));
    });

    const doChange = useEventCallback((id: string, role: string) => {
        dispatch(addContributor({ appId: appDetails.id, params: { email: id, role } }));
    });

    const doRemove = useEventCallback((id: string) => {
        dispatch(removeContributor({ appId: appDetails.id, id }));
    });

    const disabled = appDetails.role !== 'Owner' || contributorsUpdating;

    return (
        <>
            <h2>{texts.common.contributors}</h2>

            <Card>
                <CardBody>
                    <FormError error={contributorsError} />

                    {appDetails.contributors.map(x =>
                        <Row noGutters className='align-items-center mb-2' key={x.userId}>
                            <Col>
                                <span className='truncate'>{x.userName}</span>
                            </Col>
                            <Col xs={4} className='pl-2'>
                                <Input type='select' value={x.role} disabled={disabled || x.userId === userId} onChange={ev => doChange(x.userId, ev.target.value)}>
                                    <option value='Admin'>Admin</option>
                                    <option value='Owner'>Owner</option>
                                </Input>
                            </Col>
                            <Col xs='auto' className='pl-2'>
                                <Button color='danger' disabled={disabled || x.userId === userId} type='button' onClick={() => doRemove(x.userId)}>
                                    <Icon type='delete' />
                                </Button>
                            </Col>
                        </Row>,
                    )}
                </CardBody>
                <CardFooter>
                    <Row noGutters>
                        <Col>
                            <Input type='email' value={email} onChange={doSetEmail} disabled={disabled} placeholder={texts.common.inviteByEmail} />
                        </Col>
                        <Col xs='auto' className='pl-2'>
                            <Button color='success' disabled={!email || email.length === 0 || disabled} onClick={doInvite}>
                                {texts.common.invite}
                            </Button>
                        </Col>
                    </Row>
                </CardFooter>
            </Card>
        </>
    );
};
