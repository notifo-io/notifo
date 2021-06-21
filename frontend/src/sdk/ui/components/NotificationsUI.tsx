/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { h } from 'preact';

import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { addNotifications, deleteNotification, setConnected, useDispatch } from '@sdk/ui/model';
import { isFunction } from 'lodash';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { NotificationsButton } from './NotificationsButton';
import { NotificationsModal } from './NotificationsModal';
import { buildConnection } from './../api';

export interface NotificationsUIProps {
    // The main config.
    config: SDKConfig;

    // The notifications options.
    options: NotificationsOptions;
}

export const NotificationsUI = (props: NotificationsUIProps) => {
    const {
        config,
        options,
    } = props;

    const dispatch = useDispatch();
    const [isOpen, setIsOpen] = useState(false);
    const [connection] = useState(() => buildConnection(config));

    useEffect(() => {
        connection.onNotifications((notifications, isUpdate) => {
            addNotifications(notifications, dispatch);

            if (isUpdate && isFunction(config.onNotification)) {
                for (const notification of notifications) {
                    try {
                        config.onNotification(notification);
                    } catch {
                        // eslint-disable-next-line no-console
                        console.error('Failed to invoke notification callback');
                    }
                }
            }
        });

        connection.onDelete(({ id }) => {
            deleteNotification(id, dispatch);
        });

        connection.onReconnected(() => {
            setConnected(true, dispatch);
        });

        connection.onDisconnected(() => {
            setConnected(false, dispatch);
        });

        connection.start().then(() => {
            setConnected(true, dispatch);
        });
    }, [config]);

    const doConfirm = useCallback(async (notification: NotifoNotification) => {
        await connection.confirmMany([], notification.id);

        addNotifications([{ ...notification, isConfirmed: true }], dispatch);
    }, []);

    const doSee = useCallback(async (notification: NotifoNotification) => {
        await connection.confirmMany([notification.id]);

        addNotifications([{ ...notification, isSeen: true }], dispatch);
    }, []);

    const doDelete = useCallback(async (notification: NotifoNotification) => {
        await connection.delete(notification.id);
    }, []);

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    const doHide = useCallback(() => {
        setIsOpen(false);
    }, []);

    return (
        <div class='notifo'>
            <NotificationsButton options={options} onClick={doShow} />

            {isOpen &&
                <NotificationsModal
                    config={config}
                    onClickOutside={doHide}
                    onConfirm={doConfirm}
                    onDelete={doDelete}
                    onSeen={doSee}
                    options={options}
                />
            }
        </div>
    );
};
