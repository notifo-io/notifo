/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button } from 'reactstrap';
import { FormatDate, Icon, JsonDetails } from '@app/framework';
import { UserNotificationDetailsDto } from '@app/service';

export interface NotificationRowProps {
    // The notification.
    notification: UserNotificationDetailsDto;
}

export const NotificationRow = React.memo((props: NotificationRowProps) => {
    const { notification } = props;

    const [isOpen, setIsOpen] = React.useState(false);

    return (
        <>
            <tr>
                <td>
                    <Button size='sm' color='link' onClick={() => setIsOpen(!isOpen)}>
                        <Icon type={isOpen ? 'expand_less' : 'expand_more'} />
                    </Button>
                </td>
                <td>
                    <span className='truncate'>{notification.subject}</span>
                </td>
                <td>
                    <span className='truncate'>
                        <FormatDate date={notification.created} />
                    </span>
                </td>
            </tr>

            {isOpen &&
                <tr className='user-notification-details'>
                    <td className='no-padding bordered' colSpan={3}>
                        <JsonDetails object={notification} />
                    </td>
                </tr>
            }
        </>
    );
});
