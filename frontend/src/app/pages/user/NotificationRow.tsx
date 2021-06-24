/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FromNow, Icon, JsonDetails } from '@app/framework';
import { NotificationDto } from '@app/service';
import * as React from 'react';
import { Button } from 'reactstrap';

export interface NotificationRowProps {
    // The notification.
    notification: NotificationDto;
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
                        <FromNow date={notification.created} />
                    </span>
                </td>
            </tr>

            {isOpen &&
                <tr>
                    <td className='no-padding bordered' colSpan={3}>
                        <JsonDetails object={notification} />
                    </td>
                </tr>
            }
        </>
    );
});
