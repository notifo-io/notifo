/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { useEffect, useState } from 'preact/hooks';
import { isFunction, NotificationsOptions, SDKConfig } from '@sdk/shared';
import { buildConnection } from '@sdk/ui/api';
import { addNotifications, deleteNotification, setConnected, useDispatch } from '@sdk/ui/model';
import { Modal } from './Modal';
import { NotificationsButton } from './NotificationsButton';
import { NotificationsModal } from './NotificationsModal';
import { useToggle } from './utils';

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
    const modal = useToggle();
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

    return (
        <div class='notifo'>
            <NotificationsButton options={options} onClick={modal.show} />

            {modal.isOpen &&
                <Modal onClickOutside={modal.hide} position={options.position}>
                    <NotificationsModal
                        config={config}
                        connection={connection}
                        onClickOutside={modal.hide}
                        options={options}
                    />
                </Modal>
            }
        </div>
    );
};
