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
import { useState } from 'preact/hooks';
import { Modal } from './Modal';
import { Notifications } from './Notifications';
import { NotificationsArchive } from './NotificationsArchive';
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

type View = 'Notifications' | 'Archive' | 'Profile';

export const NotificationsModal = (props: NotificationsModalProps) => {
    const {
        config,
        onClickOutside,
        onConfirm,
        onDelete,
        onSeen,
        options,
    } = props;

    const [ref, setRef] = useState<HTMLDivElement | null>(null);
    const [view, setView] = useState<View>('Notifications');

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            <div ref={setRef}>
                {view === 'Profile' ? (
                    <ProfileSettings config={config} options={options}
                        onClose={() => setView('Notifications')} />

                ) : (view === 'Archive') ? (
                    <NotificationsArchive config={config} options={options}
                        onClose={() => setView('Notifications')} />
                ) : (
                    <Notifications config={config} options={options}
                        onConfirm={onConfirm}
                        onDelete={onDelete}
                        onSeen={onSeen}
                        onShowArchive={() => setView('Archive')}
                        onShowProfile={() => setView('Profile')}
                        parent={ref?.parentNode as any}
                    />
                )}
            </div>
        </Modal>
    );
};
