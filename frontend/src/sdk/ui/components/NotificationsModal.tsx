/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useState } from 'preact/hooks';
import { NotifoNotification } from './../../api';
import { NotificationsOptions, SDKConfig } from './../../shared';
import { Loader } from './Loader';
import { Modal } from './Modal';
import { NotificationItem } from './NotificationItem';

export interface NotificationsModalProps {
    // The notifications.
    notifications: ReadonlyArray<NotifoNotification>;

    // True when loaded at least once.
    loaded: boolean;

    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // True when connected to the server.
    isConnected: boolean;

    // Triggered when clicked outside.
    onClickOutside?: () => void;

    // Clicked when a notification is confirmed.
    onConfirm?: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is seen.
    onSeen?: (notification: NotifoNotification) => Promise<any>;
}

export const NotificationsModal = (props: NotificationsModalProps) => {
    const {
        config,
        loaded,
        notifications,
        onClickOutside,
        onConfirm,
        onSeen,
        options,
    } = props;

    const [ref, setRef] = useState<HTMLDivElement>(null);

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            <div ref={setRef}>
                {!loaded &&
                    <div class='notifo-loading'>
                        <Loader size={18} visible={true} />
                    </div>
                }

                {loaded && (!notifications || notifications.length === 0) &&
                    <div class='notifo-empty'>{options.textEmpty}</div>
                }

                {loaded && notifications.length > 0 &&
                    <div>
                        {notifications.map(x => (
                            <NotificationItem key={x.id} notification={x}
                                config={config}
                                modal={ref?.parentElement}
                                options={options}
                                onConfirm={onConfirm}
                                onSeen={onSeen}
                            />
                        ))}
                    </div>
                }
            </div>
        </Modal>
    );
};
