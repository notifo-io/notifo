/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ApiValue, ClickOutside, FormatDate, Icon } from '@app/framework';
import { UserDto } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import { Button, Popover, PopoverBody, Table } from 'reactstrap';

function usePopover(): [boolean, () => void, () => void, () => void] {
    const [isOpen, setIsOpen] = React.useState(false);

    const toggle = () => {
        setIsOpen(!isOpen);
    };

    const open = () => {
        setIsOpen(true);
    };

    const close = () => {
        setIsOpen(false);
    };

    return [isOpen, open, close, toggle];
}

export const ButtonSms = ({ user }: { user: UserDto }) => {
    const [isOpen, open, close, toggle] = usePopover();

    const hasValue = !!user.phoneNumber;

    return (
        <>
            <Button color='' id='buttonSms' className={clazz(hasValue)} block onClick={open}>
                <Icon type='sms' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonSms' placement='auto' toggle={toggle}>
                <ClickOutside onClickOutside={close}>
                    <PopoverBody>
                        {user.phoneNumber}
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

export const ButtonEmail = ({ user }: { user: UserDto }) => {
    const [isOpen, open, close, toggle] = usePopover();

    const hasValue = !!user.emailAddress;

    return (
        <>
            <Button color='' id='buttonEmail' className={clazz(hasValue)} block onClick={open}>
                <Icon type='mail_outline' />
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonEmail' placement='auto' toggle={toggle}>
                <ClickOutside onClickOutside={close}>
                    <PopoverBody>
                        {user.emailAddress}
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

export const ButtonWebPush = ({ user }: { user: UserDto }) => {
    const [isOpen, open, close, toggle] = usePopover();

    const hasValue = user.numberOfWebPushTokens > 0;

    return (
        <>
            <Button color='' id='buttonWebPush' className={clazz(hasValue)} block onClick={open}>
                <Icon type='browser' /> {user.numberOfWebPushTokens}
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonWebPush' placement='auto' toggle={toggle}>
                <ClickOutside onClickOutside={close}>
                    <PopoverBody>
                        {user.numberOfWebPushTokens}
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

export const ButtonMobilePush = ({ user }: { user: UserDto }) => {
    const [isOpen, open, close, toggle] = usePopover();

    const hasValue = user.numberOfMobilePushTokens > 0;

    return (
        <>
            <Button color='' id='buttonMobilePush' className={clazz(hasValue)} block onClick={open}>
                <Icon type='mobile' /> {user.numberOfMobilePushTokens}
            </Button>

            <Popover isOpen={isOpen && hasValue} target='buttonMobilePush' placement='auto' toggle={toggle} popperClassName='popper-lg'>
                <ClickOutside onClickOutside={close}>
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
                                        <td>
                                            {token.lastWakeup ? (
                                                <FormatDate format='Pp' date={token.lastWakeup} />
                                            ) : (
                                                <>{texts.common.notYet}</>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    </PopoverBody>
                </ClickOutside>
            </Popover>
        </>
    );
};

function clazz(hasValue: boolean) {
    let result = 'btn-flat pl-0 pr-0';

    if (hasValue) {
        result += ' text-primary';
    } else {
        result += ' text-muted';
    }

    return result;
}
