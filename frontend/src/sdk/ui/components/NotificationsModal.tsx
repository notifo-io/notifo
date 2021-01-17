/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { useState } from 'preact/hooks';
import { Modal } from './Modal';
import { NotificationsList } from './NotificationsList';
import { ProfileSettings } from './ProfileSettings';

export interface NotificationsModalProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // Triggered when clicked outside.
    onClickOutside?: () => void;

    // Clicked when a notification is confirmed.
    onConfirm: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is seen.
    onSeen: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is deleted.
    onDelete: (notification: NotifoNotification) => Promise<any>;
}

export const NotificationsModal = (props: NotificationsModalProps) => {
    const {
        config,
        onClickOutside,
        onConfirm,
        onDelete,
        onSeen,
        options,
    } = props;

    const [ref, setRef] = useState<HTMLDivElement>(null);
    const [showProfile, setShowProfile] = useState(false);

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            <div ref={setRef}>
                {showProfile ? (
                    <ProfileSettings config={config} options={options}
                        onShowProfile={setShowProfile} />
                ) : (
                    <NotificationsList config={config} options={options}
                        onConfirm={onConfirm}
                        onDelete={onDelete}
                        onSeen={onSeen}
                        onShowProfile={setShowProfile}
                        parent={ref?.parentNode as any}
                    />
                )}
            </div>
        </Modal>
    );
};
