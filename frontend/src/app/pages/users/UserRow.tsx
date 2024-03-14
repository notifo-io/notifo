/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { NavLink } from 'react-router-dom';
import { Button } from 'reactstrap';
import { Confirm, FormatDate, Icon } from '@app/framework';
import { UserDto } from '@app/service';
import { CounterRow } from '@app/shared/components';
import { texts } from '@app/texts';

export interface UserRowProps {
    // The user.
    user: UserDto;

    // True to show all counters.
    showCounters?: boolean;

    // The publish event.
    onPublish?: (user: UserDto) => void;

    // The edit event.
    onEdit?: (user: UserDto) => void;

    // The delete event.
    onDelete?: (user: UserDto) => void;
}

export const UserRow = React.memo((props: UserRowProps) => {
    const {
        showCounters,
        onDelete,
        onEdit,
        onPublish,
        user,
    } = props;

    const doDelete = () => {
        onDelete && onDelete(user);
    };

    const doEdit = () => {
        onEdit && onEdit(user);
    };

    const doPublish = () => {
        onPublish && onPublish(user);
    };

    return (
        <CounterRow counters={user.counters} columnCount={6} showCounters={showCounters}>
            <tr className='list-item-summary'>
                <td>
                    <NavLink to={user.id}>
                        <span className='truncate mono'>{user.id}</span>
                    </NavLink>
                </td>
                <td>
                    <NavLink to={user.id}>
                        <span className='truncate'>{user.fullName}</span>
                    </NavLink>
                </td>
                <td>
                    <NavLink to={user.id}>
                        <span className='truncate'>{user.emailAddress}</span>
                    </NavLink>
                </td>
                <td className='text-right'>
                    <FormatDate date={user.lastNotification} />
                </td>
                <td className='text-right'>
                    <FormatDate date={user.lastUpdate} />
                </td>
                <td className='text-right'>
                    <Button className='ml-1' size='sm' color='info' onClick={doPublish} data-tooltip-id="default-tooltip" data-tooltip-content={texts.common.publish}>
                        <Icon type='send' />
                    </Button>

                    <Button className='ml-1' size='sm' color='primary' onClick={doEdit} data-tooltip-id="default-tooltip" data-tooltip-content={texts.common.edit}>
                        <Icon type='create' />
                    </Button>

                    <Confirm onConfirm={doDelete} text={texts.users.confirmDelete}>
                        {({ onClick }) => (
                            <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tooltip-id="default-tooltip" data-tooltip-content={texts.common.delete}>
                                <Icon type='delete' />
                            </Button>
                        )}
                    </Confirm>
                </td>
            </tr>
        </CounterRow>
    );
});
