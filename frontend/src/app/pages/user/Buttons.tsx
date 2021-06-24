/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ApiValue, FormatDate, Icon } from '@app/framework';
import { UserDto } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import { Button, Popover, PopoverBody, Table } from 'reactstrap';

export const ButtonSms = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const toggle = () => {
        setIsOpen(!isOpen);
    };

    const hasValue = !!user.phoneNumber;

    return (
        <>
            <Button color='' id='buttonSms' className={hasValue ? 'text-primary btn-flat' : 'text-muted btn-flat'} onClick={toggle}>
                <Icon type='sms' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonSms' placement='auto' toggle={toggle}>
                <PopoverBody>
                    {user.phoneNumber}
                </PopoverBody>
            </Popover>
        </>
    );
};

export const ButtonEmail = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const toggle = () => {
        setIsOpen(!isOpen);
    };

    const hasValue = !!user.emailAddress;

    return (
        <>
            <Button color='' id='buttonEmail' className={hasValue ? 'text-primary btn-flat' : 'text-muted btn-flat'} onClick={toggle}>
                <Icon type='mail_outline' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonEmail' placement='auto' toggle={toggle}>
                <PopoverBody>
                    {user.emailAddress}
                </PopoverBody>
            </Popover>
        </>
    );
};

export const ButtonWebPush = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const toggle = () => {
        setIsOpen(!isOpen);
    };

    const hasValue = user.numberOfWebPushTokens > 0;

    return (
        <>
            <Button color='' id='buttonWebPush' className={hasValue ? 'text-primary btn-flat' : 'text-muted btn-flat'} onClick={toggle}>
                <Icon type='browser' /> {user.numberOfWebPushTokens}
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonWebPush' placement='auto' toggle={toggle}>
                <PopoverBody>
                    {user.numberOfWebPushTokens}
                </PopoverBody>
            </Popover>
        </>
    );
};

export const ButtonMobilePush = ({ user }: { user: UserDto }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const toggle = () => {
        setIsOpen(!isOpen);
    };

    const hasValue = user.numberOfMobilePushTokens > 0;

    return (
        <>
            <Button color='' id='buttonMobilePush' className={hasValue ? 'text-primary btn-flat' : 'text-muted btn-flat'} onClick={toggle}>
                <Icon type='mobile' /> {user.numberOfMobilePushTokens}
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonMobilePush' placement='auto' toggle={toggle} popperClassName='popper-lg'>
                <PopoverBody>
                    <Table className='table-middle table-fixed condensed' size='sm' borderless>
                        <colgroup>
                            <col style={{ width: 100 }} />
                            <col />
                            <col style={{ width: 170 }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>{texts.common.device}</th>
                                <th>{texts.common.token}</th>
                                <th>{texts.common.lastWakeup}</th>
                            </tr>
                        </thead>

                        <tbody>
                            {user.mobilePushTokens.map((token, i) => (
                                <tr key={i}>
                                    <td>{token.deviceType}</td>
                                    <td>
                                        <ApiValue size='sm' value={token.token} />
                                    </td>
                                    <td className='text-right'>
                                        <FormatDate format='Pp' date={token.lastWakeup} />
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </PopoverBody>
            </Popover>
        </>
    );
};
