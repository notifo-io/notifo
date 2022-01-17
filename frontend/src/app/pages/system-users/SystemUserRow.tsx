/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button } from 'reactstrap';
import {  Confirm, Icon } from '@app/framework';
import { SystemUserDto } from '@app/service';
import { texts } from '@app/texts';

export interface SystemUserRowProps {
    // The system user.
    user: SystemUserDto;

    // True to show all counters.
    showCounters?: boolean;

    // The lock event.
    onLock?: (systemUser: SystemUserDto) => void;

    // The unlock event.
    onUnlock?: (systemUser: SystemUserDto) => void;

    // The edit event.
    onEdit?: (systemUser: SystemUserDto) => void;

    // The delete event.
    onDelete?: (systemUser: SystemUserDto) => void;
}

export const SystemUserRow = React.memo((props: SystemUserRowProps) => {
    const {
        onDelete,
        onEdit, 
        onLock,
        onUnlock,
        user,
    } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = React.useCallback(() => {
        onDelete && onDelete(user);
    }, [onDelete, user]);

    const doEdit = React.useCallback(() => {
        onEdit && onEdit(user);
    }, [onEdit, user]);

    const doLock = React.useCallback(() => {
        onLock && onLock(user);
    }, [onLock, user]);

    const doUnlock = React.useCallback(() => {
        onUnlock && onUnlock(user);
    }, [onUnlock, user]);

    return (
        <>
            <tr className='list-item-summary'>
                <td>
                    <span className='truncate'>{user.email}</span>
                </td>
                <td>
                    {user.roles.find(x => x.toUpperCase() === 'ADMIN') ? texts.common.yes : '-'}
                </td>
                <td className='text-right'>
                    {user.canUpdate &&
                        <>
                            {!user.isLocked &&
                                <Button className='ml-1' size='sm' color='light' onClick={doLock} data-tip={texts.users.lock}>
                                    <Icon type='lock_outline' />
                                </Button>
                            }

                            {user.isLocked &&
                                <Button className='ml-1' size='sm' color='dark' onClick={doUnlock} data-tip={texts.users.unlock}>
                                    <Icon type='lock_open' />
                                </Button>
                            }

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
                        </>
                    }
                </td>
            </tr>

            <tr className='list-item-separator'></tr>
        </>
    );
});
