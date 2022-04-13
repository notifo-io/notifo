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
import { NotificationsOptions, SDKConfig } from '@sdk/shared';
import { Connection } from '@sdk/ui/api';
import { loadTopics, useDispatch, useStore } from '@sdk/ui/model';
import { ArchiveView } from './ArchiveView';
import { NotificationsView } from './NotificationsView';
import { ProfileView } from './ProfileView';
import { TopicsView } from './TopicsView';

export interface NotificationsModalProps {
    // The main config.
    config: SDKConfig;

    // The connection.
    connection: Connection;

    // The options.
    options: NotificationsOptions;

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

type View = 'Notifications' | 'Archive' | 'Profile' | 'Topics';

export const NotificationsModal = (props: NotificationsModalProps) => {
    const {
        config,
        connection,
        options,
    } = props;

    const dispatch = useDispatch();
    const topics = useStore(x => x.topics);
    const [viewRoot, setViewRoot] = useState<HTMLDivElement | null>(null);
    const [viewMode, setViewMode] = useState<View>('Notifications');

    useEffect(() => {
        dispatch(loadTopics(config));
    }, [dispatch, config]);

    return (
        <div ref={setViewRoot}>
            <ul class='notifo-tabs'>
                <li class={makeActive(viewMode, 'Notifications')} onClick={() => setViewMode('Notifications')}>
                    {config.texts.notifications}
                </li>
                <li class={makeActive(viewMode, 'Archive')} onClick={() => setViewMode('Archive')}>
                    {config.texts.archive}
                </li>

                {topics.length > 0 &&
                    <li class={makeActive(viewMode, 'Topics')} onClick={() => setViewMode('Topics')}>
                        {config.texts.topics}
                    </li>
                }

                {(config.allowProfile && !options.hideProfile) &&
                    <li class={makeActive(viewMode, 'Profile')} onClick={() => setViewMode('Profile')}>
                        {config.texts.profile}
                    </li>
                }
            </ul>

            <div class='notifo-tabs-content'>
                {viewMode === 'Profile' ? (
                    <ProfileView config={config} options={options} />
                ) : (viewMode === 'Archive') ? (
                    <ArchiveView config={config} options={options} />
                ) : (viewMode === 'Topics') ? (
                    <TopicsView config={config} options={options} />
                ) : (viewMode === 'Notifications') ? (
                    <NotificationsView 
                        config={config} 
                        connection={connection}
                        options={options}
                        parent={viewRoot?.parentNode as any}
                    />
                ) : null}
            </div>
        </div>
    );
};

function makeActive(view: View, currentView: View) {
    return view === currentView ? 'active' : undefined;
}
