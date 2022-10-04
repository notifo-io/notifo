/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Popover, PopoverBody, Table } from 'reactstrap';
import { ApiValue, ClickOutside, FormatDate, Icon, useBoolean, useEventCallback } from '@app/framework';
import { UserDto } from '@app/service';
import { deleteUserMobilePushToken, deleteUserWebPushSubscription } from '@app/state';
import { texts } from '@app/texts';

export const ButtonSms = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = useBoolean();

    const hasValue = !!user.phoneNumber;

    return (
        <>
            <Button color='none' id='buttonSms' className={getButtonClass(hasValue)} block onClick={setIsOpen.on}>
                <Icon type='sms' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonSms' placement='auto' toggle={setIsOpen.toggle}>
                <ClickOutside onClickOutside={close} isActive={true}>
                    <PopoverBody>
                        <h5>{texts.common.phoneNumber}</h5>

                        {user.phoneNumber}
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

export const ButtonEmail = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = useBoolean();

    const hasValue = !!user.emailAddress;

    return (
        <>
            <Button color='none' id='buttonEmail' className={getButtonClass(hasValue)} block onClick={setIsOpen.on}>
                <Icon type='mail_outline' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonEmail' placement='auto' toggle={setIsOpen.toggle}>
                <ClickOutside onClickOutside={close} isActive={true}>
                    <PopoverBody>
                        <h5>{texts.common.emailAddress}</h5>

                        {user.emailAddress}
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

export const ButtonWebPush = ({ appId, user }: { appId: string; user: UserDto }) => {
    const dispatch = useDispatch();
    const [isOpen, setIsOpen] = useBoolean();

    const doDelete = useEventCallback((endpoint: string) => {
        dispatch(deleteUserWebPushSubscription({ appId, userId: user.id, endpoint }));
    });

    const hasValue = user.webPushSubscriptions.length > 0;

    return (
        <>
            <Button color='none' id='buttonWebPush' className={getButtonClass(hasValue)} block onClick={setIsOpen.on}>
                <Icon type='browser' />

                {user.webPushSubscriptions.length > 0 &&
                    <>&nbsp;{user.webPushSubscriptions.length}</>
                }
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonWebPush' placement='auto' toggle={setIsOpen.toggle} popperClassName='popper-lg'>
                {({ scheduleUpdate }) => 
                    <ClickOutside onClickOutside={close} isActive={true}>
                        <PopoverBody>
                            <h5>{texts.common.webPush}</h5>

                            <Table className='table-middle table-fixed condensed' size='sm' borderless>
                                <colgroup>
                                    <col />
                                    <col style={{ width: 40 }} />
                                </colgroup>

                                <thead>
                                    <tr>
                                        <th>{texts.common.token}</th>
                                        <th>&nbsp;</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {user.webPushSubscriptions.map((subscription, i) => (
                                        <tr key={i}>
                                            <td>
                                                <ApiValue size='sm' value={subscription.endpoint} />
                                            </td>
                                            <td>            
                                                <Button size='sm' color='danger' onClick={() => { doDelete(subscription.endpoint); scheduleUpdate(); }}>
                                                    <Icon type='delete' />
                                                </Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </PopoverBody>
                    </ClickOutside>
                }
            </Popover>
                               
        </>
    );
};

export const ButtonMobilePush = ({ appId, user }: { appId: string; user: UserDto }) => {
    const dispatch = useDispatch();
    const [isOpen, setIsOpen] = useBoolean();

    const doDelete = useEventCallback((token: string) => {
        dispatch(deleteUserMobilePushToken({ appId, userId: user.id, token }));
    });

    const hasValue = user.mobilePushTokens.length > 0;

    return (
        <>
            <Button color='none' id='buttonMobilePush' className={getButtonClass(hasValue)} block onClick={setIsOpen.on}>
                <Icon type='mobile' />

                {user.mobilePushTokens.length > 0 &&
                    <>&nbsp;{user.mobilePushTokens.length}</>
                }
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonMobilePush' placement='auto' toggle={setIsOpen.toggle} popperClassName='popper-lg'>
                {({ scheduleUpdate }) => 
                    <ClickOutside onClickOutside={close} isActive={true}>
                        <PopoverBody>
                            <h5>{texts.common.mobilePush}</h5>

                            <Table className='table-middle table-fixed condensed' size='sm' borderless>
                                <colgroup>
                                    <col style={{ width: 100 }} />
                                    <col />
                                    <col style={{ width: 170 }} />
                                    <col style={{ width: 40 }} />
                                </colgroup>

                                <thead>
                                    <tr>
                                        <th>{texts.common.device}</th>
                                        <th>{texts.common.token}</th>
                                        <th>{texts.common.lastWakeup}</th>
                                        <th>&nbsp;</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {user.mobilePushTokens.map((token, i) => (
                                        <tr key={i}>
                                            <td>{token.deviceType}</td>
                                            <td>
                                                <ApiValue size='sm' value={token.token} />
                                            </td>
                                            <td>
                                                {token.lastWakeup ? (
                                                    <FormatDate format='Pp' date={token.lastWakeup} />
                                                ) : (
                                                    <>{texts.common.notYet}</>
                                                )}
                                            </td>
                                            <td>            
                                                <Button size='sm' color='danger' onClick={() => { doDelete(token.token); scheduleUpdate(); }}>
                                                    <Icon type='delete' />
                                                </Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </PopoverBody>
                    </ClickOutside>
                }
            </Popover>
        </>
    );
};

function getButtonClass(hasValue: boolean) {
    return classNames('btn-flat btn-font-lg', 'pl-0', 'pr-0', { 'text-primary': hasValue, 'text-muted': !hasValue });
}
