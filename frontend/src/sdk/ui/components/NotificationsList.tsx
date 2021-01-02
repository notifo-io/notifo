/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';
import { useCallback } from 'preact/hooks';
import { useNotifoState } from '../model';

import { NotifoNotification } from './../../api';
import { NotificationsOptions, SDKConfig } from './../../shared';
import { Icon } from './Icon';
import { Loader } from './Loader';
import { NotificationItem } from './NotificationItem';

export interface NotificationsListProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // The html parent.
    parent?: HTMLElement;

    // Clicked when a notification is confirmed.
    onConfirm?: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is seen.
    onSeen?: (notification: NotifoNotification) => Promise<any>;

    // To toggle the profile view.
    onShowProfile?: (show: boolean) => void;
}

export const NotificationsList = (props: NotificationsListProps) => {
    const {
        config,
        onConfirm,
        onSeen,
        onShowProfile,
        options,
        parent,
    } = props;

    const [state] = useNotifoState();
    const notifications = state.notifications;
    const isLoaded = state.notificationsTransition === 'InProgress';
    const isConnected = state.isConnected;

    const doShowProfile = useCallback(() => {
        onShowProfile && onShowProfile(false);
    }, [onShowProfile]);

    return (
        <div>
            <div>
                <button type='button' onClick={doShowProfile}>
                    <Icon type='back' size={20} />
                </button>
            </div>

            {!isConnected || !isLoaded &&
                <div class='notifo-loading'>
                    <Loader size={18} visible={true} />
                </div>
            }

            {isLoaded && (!isLoaded || notifications.length === 0) &&
                <div class='notifo-empty'>{config.texts.notificationsEmpty}</div>
            }

            {isLoaded && notifications.length > 0 &&
                <div>
                    {notifications.map(x => (
                        <NotificationItem key={x.id}
                            config={config}
                            notification={x}
                            options={options}
                            onConfirm={onConfirm}
                            onSeen={onSeen}
                            modal={parent}
                        />
                    ))}
                </div>
            }
        </div>
    );
};
