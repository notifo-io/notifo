/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { Connection } from '@sdk/ui/api/connection';
import { addNotifications, setConnected, useDispatch } from '@sdk/ui/model';
import { isFunction } from 'lodash';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { NotificationsButton } from './NotificationsButton';
import { NotificationsModal } from './NotificationsModal';

export interface NotificationsProps {
    // The main config.
    config: SDKConfig;

    // The notifications options.
    options: NotificationsOptions;
}

export const NotificationsContainer = (props: NotificationsProps) => {
    const {
        config,
        options,
    } = props;

    const dispatch = useDispatch();
    const [isOpen, setIsOpen] = useState(false);
    const [connection] = useState(() => new Connection(config));

    useEffect(() => {
        connection.onNotifications(notifications => {
            addNotifications(notifications, dispatch);
        });

        connection.onNotification(notification => {
            addNotifications([notification], dispatch);

            if (isFunction(config.onNotification)) {
                try {
                    config.onNotification(notification);
                } catch {
                    console.error('Failed to invoke notification callback');
                }
            }
        });

        connection.onReconnected(() => {
            setConnected(true, dispatch);
        });

        connection.onReconnecting(() => {
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

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    const doHide = useCallback(() => {
        setIsOpen(false);
    }, []);

    return (
        <div className='notifo'>
            <NotificationsButton options={options} onClick={doShow} />

            {isOpen &&
                <NotificationsModal
                    config={config}
                    onClickOutside={doHide}
                    onConfirm={doConfirm}
                    onSeen={doSee}
                    options={options}
                />
            }
        </div>
    );
};
