/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button, Nav, NavItem, NavLink } from 'reactstrap';
import { Code, FormatDate, Icon } from '@app/framework';
import { UserNotificationDetailsDto } from '@app/service';
import { texts } from '@app/texts';
import { NotificationDetails } from './NotificationDetails';

export interface NotificationRowProps {
    // The notification.
    notification: UserNotificationDetailsDto;
}

export const NotificationRow = React.memo((props: NotificationRowProps) => {
    const { notification } = props;

    const [isOpen, setIsOpen] = React.useState(0);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });
    
    const nonWebStatus = Object.entries(props.notification.channels).filter(x => x[0] !== 'web').map(x => x[1]);
    const numHandled = nonWebStatus.filter(x => !!Object.values(x.status).find(x => x.status === 'Handled')).length;
    const numFirstDelivered = nonWebStatus.filter(x => !!x.firstDelivered).length;
    const numFirstSeen = nonWebStatus.filter(x => !!x.firstSeen).length;
    const numFirstConfirmed = nonWebStatus.filter(x => !!x.firstConfirmed).length;

    return (
        <>
            <tr>
                <td>
                    <Button size='sm' color='link' onClick={() => setIsOpen(x => x === 0 ? 1 : 0)}>
                        <Icon type={isOpen ? 'expand_less' : 'expand_more'} />
                    </Button>
                </td>
                <td>
                    <span className='truncate'>{notification.subject}</span>
                </td>
                <td data-tip={texts.common.handled}>
                    {numHandled || '-'}
                </td>
                <td data-tip={texts.common.delivered}>
                    {numFirstDelivered || '-'}
                </td>
                <td data-tip={texts.common.seen}>
                    {numFirstSeen || '-'}
                </td>
                <td data-tip={texts.common.confirmed}>
                    {numFirstConfirmed || '-'}
                </td>
                <td className='text-right'>
                    <span className='truncate'>
                        <FormatDate format='Ppp' date={notification.created} />
                    </span>
                </td>
            </tr>

            {isOpen > 0 &&
                <>
                    <tr className='user-notification-header'>
                        <td className='no-padding bordered' colSpan={7}>
                            <Nav className='nav-tabs2'>
                                <NavItem>
                                    <NavLink active={isOpen === 1} onClick={() => setIsOpen(1)}>
                                        {texts.common.overview}
                                    </NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink active={isOpen === 2} onClick={() => setIsOpen(2)}>
                                        JSON
                                    </NavLink>
                                </NavItem>
                            </Nav>
                        </td>
                    </tr>
                    <tr className='user-notification-details'>
                        <td className={classNames('bordered', { 'no-padding': isOpen === 2 })} colSpan={7}>
                            {isOpen === 1 ? (
                                <NotificationDetails notification={notification} />
                            ) : (
                                <Code value={notification} />
                            )}
                        </td>
                    </tr>
                </>

            }
        </>
    );
});
