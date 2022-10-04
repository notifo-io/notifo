/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { match } from 'react-router';
import { NavLink } from 'react-router-dom';
import ReactTooltip from 'react-tooltip';
import { Button } from 'reactstrap';
import { combineUrl, Confirm, FormatDate, Icon } from '@app/framework';
import { UserDto } from '@app/service';
import { CounterRow } from '@app/shared/components';
import { texts } from '@app/texts';

export interface UserRowProps {
    // The user.
    user: UserDto;

    // The match.
    match: match;

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
        match, 
        onDelete,
        onEdit,
        onPublish,
        user,
    } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = () => {
        onDelete && onDelete(user);
    };

    const doEdit = () => {
        onEdit && onEdit(user);
    };

    const doPublish = () => {
        onPublish && onPublish(user);
    };

    const url = combineUrl(match.url, user.id);

    return (
        <CounterRow counters={user.counters} columnCount={6} showCounters={showCounters}>
            <tr className='list-item-summary'>
                <td>
                    <NavLink to={url}>
                        <span className='truncate mono'>{user.id}</span>
                    </NavLink>
                </td>
                <td>
                    <NavLink to={url}>
                        <span className='truncate'>{user.fullName}</span>
                    </NavLink>
                </td>
                <td>
                    <NavLink to={url}>
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
                    <Button className='ml-1' size='sm' color='info' onClick={doPublish} data-tip={texts.common.publish}>
                        <Icon type='send' />
                    </Button>

                    <Button className='ml-1' size='sm' color='primary' onClick={doEdit} data-tip={texts.common.edit}>
                        <Icon type='create' />
                    </Button>

                    <Confirm onConfirm={doDelete} text={texts.users.confirmDelete}>
                        {({ onClick }) => (
                            <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                                <Icon type='delete' />
                            </Button>
                        )}
                    </Confirm>
                </td>
            </tr>
        </CounterRow>
    );
});
