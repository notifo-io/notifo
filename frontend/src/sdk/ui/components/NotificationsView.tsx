/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { NotificationItem } from './NotificationItem';

export interface NotificationsViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // The html parent.
    parent?: HTMLElement;

    // Clicked when a notification is confirmed.
    onConfirm: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is seen.
    onSeen: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is deleted.
    onDelete: (notification: NotifoNotification) => Promise<any>;
}

export const NotificationsView = (props: NotificationsViewProps) => {
    const {
        config,
        onConfirm,
        onDelete,
        onSeen,
        options,
        parent,
    } = props;

    const notifications = useStore(x => x.notifications);
    const isLoaded = useStore(x => x.notificationsStatus !== 'InProgress');
    const isConnected = useStore(x => x.isConnected);

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
                            onConfirm={onConfirm}
                            onDelete={onDelete}
                            onSeen={onSeen}
                            modal={parent}
                        />
                    ))}
                </div>
            }
        </Fragment>
    );
};
