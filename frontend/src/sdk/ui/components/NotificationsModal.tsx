/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { useState } from 'preact/hooks';
import { NotificationsOptions, NotifoNotification, SDKConfig } from '@sdk/shared';
import { ArchiveView } from './ArchiveView';
import { Modal } from './Modal';
import { NotificationsView } from './NotificationsView';
import { ProfileView } from './ProfileView';
import { TopicsView } from './TopicsView';

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

type View = 'Notifications' | 'Archive' | 'Profile' | 'Topics';

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
                <ul class='notifo-tabs'>
                    <li class={makeActive(view, 'Notifications')} onClick={() => setView('Notifications')}>
                        {config.texts.notifications}
                    </li>
                    <li class={makeActive(view, 'Archive')} onClick={() => setView('Archive')}>
                        {config.texts.archive}
                    </li>
                    <li class={makeActive(view, 'Topics')} onClick={() => setView('Topics')}>
                        {config.texts.topics}
                    </li>

                    {config.allowProfile &&
                        <li class={makeActive(view, 'Profile')} onClick={() => setView('Profile')}>
                            {config.texts.profile}
                        </li>
                    }
                </ul>

                <div class='notifo-tabs-content'>
                    {view === 'Profile' ? (
                        <ProfileView config={config} options={options} />
                    ) : (view === 'Archive') ? (
                        <ArchiveView config={config} options={options} />
                    ) : (view === 'Topics') ? (
                        <TopicsView config={config} options={options} />
                    ) : (view === 'Notifications') ? (
                        <NotificationsView config={config} options={options}
                            onConfirm={onConfirm}
                            onDelete={onDelete}
                            onSeen={onSeen}
                            parent={ref?.parentNode as any}
                        />
                    ) : null}
                </div>
            </div>
        </Modal>
    );
};

function makeActive(view: View, currentView: View) {
    return view === currentView ? 'active' : undefined;
}
