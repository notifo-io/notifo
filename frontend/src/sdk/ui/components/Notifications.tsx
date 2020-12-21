/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { isFunction } from 'lodash';
import { h } from 'preact';

import { useCallback, useEffect, useState } from 'preact/hooks';
import { NotifoNotification } from './../../api';
import { NotificationsOptions, SDKConfig } from './../../shared';
import { Connection } from './../api/connection';
import { addNotifications, getUnseen, useNotifoState } from './../model';
import { NotificationsButton } from './NotificationsButton';
import { NotificationsModal } from './NotificationsModal';

export interface NotificationsProps {
    // The main config.
    config: SDKConfig;

    // The notifications options.
    options: NotificationsOptions;
}

export const NotificationsContainer = (props: NotificationsProps) => {
    const { config, options } = props;

    const [state, dispatch] = useNotifoState();
    const [isOpen, setIsOpen] = useState(false);
    const [isConnected, setIsConnected] = useState(false);
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
            setIsConnected(true);
        });

        connection.onReconnecting(() => {
            setIsConnected(false);
        });

        connection.start().then(() => {
            setIsConnected(true);
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

    const unseen = getUnseen(state);

    return (
        <div className='notifo'>
            <NotificationsButton options={options} unseen={unseen} onClick={doShow} />

            {isOpen &&
                <NotificationsModal
                    config={config}
                    loaded={true}
                    isConnected={isConnected}
                    notifications={state.notifications}
                    onClickOutside={doHide}
                    onConfirm={doConfirm}
                    onSeen={doSee}
                    options={options}
                />
            }
        </div>
    );
};
