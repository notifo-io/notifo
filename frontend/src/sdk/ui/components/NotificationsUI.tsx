/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { isFunction } from 'lodash';
import { h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { addNotifications, deleteNotification, setConnected, useDispatch } from '@sdk/ui/model';
import { buildConnection } from './../api';
import { NotificationsButton } from './NotificationsButton';
import { NotificationsModal } from './NotificationsModal';

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
            dispatch(addNotifications(notifications));

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
            dispatch(deleteNotification(id));
        });

        connection.onReconnected(() => {
            dispatch(setConnected(true));
        });

        connection.onDisconnected(() => {
            dispatch(setConnected(false));
        });

        connection.start().then(() => {
            dispatch(setConnected(true));
        });
    }, [dispatch, config, connection]);

    const doConfirm = useCallback(async (notification: NotifoNotification) => {
        await connection.confirmMany([], notification.id);

        dispatch(addNotifications([{ ...notification, isConfirmed: true }]));
    }, [dispatch, connection]);

    const doSee = useCallback(async (notification: NotifoNotification) => {
        await connection.confirmMany([notification.id]);

        dispatch(addNotifications([{ ...notification, isSeen: true }]));
    }, [dispatch, connection]);

    const doDelete = useCallback(async (notification: NotifoNotification) => {
        await connection.delete(notification.id);
    }, [connection]);

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
