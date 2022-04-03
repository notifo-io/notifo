/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback } from 'preact/hooks';
import { NotificationsOptions, NotifoNotificationDto, SDKConfig } from '@sdk/shared';
import { Connection } from '@sdk/ui/api';
import { addNotifications, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { NotificationItem } from './NotificationItem';

export interface NotificationsViewProps {
    // The main config.
    config: SDKConfig;

    // The connection.
    connection: Connection;

    // The options.
    options: NotificationsOptions;

    // The html parent.
    parent?: HTMLElement;
}

export const NotificationsView = (props: NotificationsViewProps) => {
    const {
        config,
        connection,
        options,
        parent,
    } = props;

    const dispatch = useDispatch();
    const notifications = useStore(x => x.notifications);
    const isLoaded = useStore(x => x.notificationsStatus !== 'InProgress');
    const isConnected = useStore(x => x.isConnected);

    const doConfirm = useCallback(async (notification: NotifoNotificationDto) => {
        await connection.confirmMany([], notification.id);

        dispatch(addNotifications([{ ...notification, isConfirmed: true }]));
    }, [dispatch, connection]);

    const doSee = useCallback(async (notification: NotifoNotificationDto) => {
        await connection.confirmMany([notification.id]);

        dispatch(addNotifications([{ ...notification, isSeen: true }]));
    }, [dispatch, connection]);

    const doDelete = useCallback(async (notification: NotifoNotificationDto) => {
        await connection.delete(notification.id);
    }, [connection]);

    return (
        <Fragment>
            {!isConnected || !isLoaded &&
                <div class='notifo-list-loading'>
                    <Loader size={18} visible={true} />
                </div>
            }

            {isLoaded && notifications.length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
            }

            {isLoaded && notifications.length > 0 &&
                <div>
                    {notifications.map(x => (
                        <NotificationItem key={x.id}
                            config={config}
                            notification={x}
                            options={options}
                            onConfirm={doConfirm}
                            onDelete={doDelete}
                            onSeen={doSee}
                            modal={parent}
                        />
                    ))}
                </div>
            }
        </Fragment>
    );
};
