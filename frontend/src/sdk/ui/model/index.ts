/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: no-parameter-reassignment

import { useEffect, useState } from 'preact/hooks';
import { apiGetArchive, apiGetProfile, apiGetSubscriptions, apiGetTopics, apiPostProfile, apiPostSubscriptions, NotifoNotificationDto, ProfileDto, SDKConfig, SubscriptionDto, SubscriptionsDto, TopicDto, UpdateProfileDto } from '@sdk/shared';
import { Store } from './store';

export type Status = 'InProgress' | 'Failed' | 'Success';

export interface SubscriptionState {
    // The load state.
    loadingStatus: Status;

    // The load state.
    updateStatus?: Status;

    // The subscription.
    subscription?: SubscriptionDto | null;
}

export type SubscriptionsState = { [prefix: string]: SubscriptionState };

export interface NotifoState {
    // The current notifications.
    archive: ReadonlyArray<NotifoNotificationDto>;

    // The loading status of the archive.
    archiveLoading: Status;

    // Indicates if the archive has been loaded once.
    archiveLoaded: boolean;

    // The current notifications.
    notifications: ReadonlyArray<NotifoNotificationDto>;

    // The notifications state.
    notificationsStatus: Status;

    // The loaded profile
    profile?: ProfileDto;

    // The loading status of the profile.
    profileLoading: Status;

    // Indicates whether the profile has been loaded once.
    profileLoaded: boolean;

    // The updating status of the profile.
    profileUpdating?: Status;

    // True when connected.
    isConnected?: boolean;

    // The subscriptions.
    subscriptions: SubscriptionsState;

    // The loaded topics.
    topics: TopicDto[];

    // The loading status of the topics.
    topicsLoading: Status;

    // Indicates whether the topics have been loaded once.
    topicsLoaded: boolean;
}

type NotifoAction = { type: NotifoActionType; [x: string]: any };

type NotifoActionType =
    'Connected' |
    'Disconnected' |
    'LoadProfileFailed' |
    'LoadProfileStarted' |
    'LoadProfileSuccess' |
    'LoadSubscriptionsFailed' |
    'LoadSubscriptionsStarted' |
    'LoadSubscriptionsSuccess' |
    'LoadTopicsFailed' |
    'LoadTopicsStarted' |
    'LoadTopicsSuccess' |
    'LoadArchiveFailed' |
    'LoadArchiveStarted' |
    'LoadArchiveSuccess' |
    'NotificationRemove' |
    'NotificationsAdd' |
    'SaveProfileFailed' |
    'SaveProfileStarted' |
    'SaveProfileSuccess' |
    'SetConnected' |
    'SubscribeFailed' |
    'SubscribeStarted' |
    'SubscribeSuccess';

function reducer(state: NotifoState, action: NotifoAction): NotifoState {
    switch (action.type) {
        case 'SetConnected':
            return { ...state, isConnected: action.isConnected };
        case 'LoadProfileStarted':
            return { ...state, profileLoading: 'InProgress' };
        case 'LoadProfileFailed':
            return { ...state, profileLoading: 'Failed', profileLoaded: true };
        case 'LoadProfileSuccess':
            return { ...state, profileLoading: 'Success', profileLoaded: true, profile: action.profile };
        case 'SaveProfileStarted':
            return { ...state, profileUpdating: 'InProgress' };
        case 'SaveProfileFailed':
            return { ...state, profileUpdating: 'Failed' };
        case 'SaveProfileSuccess':
            return { ...state, profileUpdating: 'Success', profile: action.profile };
        case 'LoadArchiveStarted':
            return { ...state, archiveLoading: 'InProgress' };
        case 'LoadArchiveFailed':
            return { ...state, archiveLoading: 'Failed', archiveLoaded: true };
        case 'LoadArchiveSuccess':
            return { ...state, archiveLoading: 'Success', archiveLoaded: true, archive: action.notifications };
        case 'LoadTopicsStarted':
            return { ...state, topicsLoading: 'InProgress' };
        case 'LoadTopicsFailed':
            return { ...state, topicsLoading: 'Failed', topicsLoaded: true };
        case 'LoadTopicsSuccess':
            return { ...state, topicsLoading: 'Success', topicsLoaded: true, topics: action.topics };
        case 'LoadSubscriptionsStarted': {
            const subscriptions = { ...state.subscriptions };

            for (const topic of action.topics) {
                const existing = subscriptions[topic] || {};

                subscriptions[topic] = { ...existing, loadingStatus: 'InProgress' };
            }

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionsFailed': {
            const subscriptions = { ...state.subscriptions };

            for (const topic of action.topics) {
                const existing = subscriptions[topic] || {};

                subscriptions[topic] = { ...existing, loadingStatus: 'Failed' };
            }

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionsSuccess': {
            const subscriptions = { ...state.subscriptions };

            for (const [topic, subscription] of Object.entries(action.subscriptions as SubscriptionsDto)) {
                subscriptions[topic] = { loadingStatus: 'Success', subscription };
            }

            return { ...state, subscriptions };
        }
        case 'SubscribeStarted': {
            const subscriptions = { ...state.subscriptions };

            for (const [topic, subscription] of Object.entries(action.subscriptions as SubscriptionsDto)) {
                subscriptions[topic] = { loadingStatus: 'Success', updateStatus: 'InProgress', subscription };
            }

            return { ...state, subscriptions };
        }
        case 'SubscribeFailed': {
            const subscriptions = { ...state.subscriptions };

            for (const [topic, subscription] of Object.entries(action.subscriptions as SubscriptionsDto)) {
                subscriptions[topic] = { ...subscriptions[topic] || {}, updateStatus: 'Failed', subscription };
            }

            return { ...state, subscriptions };
        }
        case 'SubscribeSuccess': {
            const subscriptions = { ...state.subscriptions };

            for (const [topic, subscription] of Object.entries(action.subscriptions as SubscriptionsDto)) {
                subscriptions[topic] = { ...subscriptions[topic] || {}, updateStatus: 'Success', subscription };
            }

            return { ...state, subscriptions };
        }
        case 'NotificationRemove': {
            const notifications = state.notifications.filter(x => x.id !== action.id);

            return { ...state, notifications };
        }
        case 'NotificationsAdd': {
            const newNotifications: ReadonlyArray<NotifoNotificationDto> = action.notifications;

            if (newNotifications.length === 0) {
                return { ...state, notificationsStatus: 'Success' };
            }

            const notifications = [...state.notifications];

            for (const notification of newNotifications) {
                if (!notification.silent) {
                    const index = notifications.findIndex(x => x.id === notification.id);

                    if (index >= 0) {
                        notifications[index] = notification;
                    } else {
                        notifications.push(notification);
                    }
                }
            }

            notifications.sort((a, b) => {
                const x = b.created!;
                const y = a.created!;

                return x > y ? 1 : x < y ? -1 : 0;
            });

            return { ...state, notifications, notificationsStatus: 'Success' };
        }
    }

    return state;
}

const initialState: NotifoState = {
    archive: [],
    archiveLoaded: false,
    archiveLoading: 'InProgress',
    isConnected: false,
    notifications: [],
    notificationsStatus: 'InProgress',
    profile: undefined,
    profileLoaded: false,
    profileLoading: 'InProgress',
    subscriptions: {},
    topics: [],
    topicsLoaded: false,
    topicsLoading: 'InProgress',
};

const store = new Store<NotifoState, NotifoAction>(initialState, reducer);

export function useDispatch() {
    return store.dispatch;
}

export function useStore<T>(selector: (state: NotifoState) => T) {
    const [state, setState] = useState(() => selector(store.current));

    useEffect(() => {
        const listener = (newState: NotifoState) => {
            setState(selector(newState));
        };

        store.subscribe(listener);

        return () => {
            store.unsubscribe(listener);
        };
    }, [selector]);

    return state;
}

export function getUnseen(state: NotifoState) {
    let count = 0;

    for (const notification of state.notifications) {
        if (notification.trackSeenUrl && !notification.isSeen) {
            count++;
        }
    }

    return count;
}

function mapStatus(statuses: (Status | undefined)[]) {
    if (statuses.indexOf('InProgress') >= 0) {
        return 'InProgress';
    } else if (statuses.indexOf('Failed') >= 0) {
        return 'Failed';
    } else {
        return 'Success';
    }
}

export function getTopicsLoadingStatus(state: NotifoState): Status {
    const statuses = [state.topicsLoading, ...state.topics.map(x => state.subscriptions[x.path]?.loadingStatus)];

    return mapStatus(statuses);
}

export function getTopicsUpdateStatus(state: NotifoState): Status {
    const statuses = state.topics.map(x => state.subscriptions[x.path]?.updateStatus);

    return mapStatus(statuses);
}

export function loadProfile(config: SDKConfig) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        try {
            store.dispatch({ type: 'LoadProfileStarted' });

            const profile = await apiGetProfile(config);

            store.dispatch({ type: 'LoadProfileSuccess', profile });
        } catch (ex) {
            store.dispatch({ type: 'LoadProfileFailed', ex });
        }
    };
}

export function loadArchive(config: SDKConfig) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        try {
            store.dispatch({ type: 'LoadArchiveStarted' });

            const notifications = await apiGetArchive(config);

            store.dispatch({ type: 'LoadArchiveSuccess', notifications });
        } catch (ex) {
            store.dispatch({ type: 'LoadArchiveFailed', ex });
        }
    };
}

export function loadTopics(config: SDKConfig) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        try {
            store.dispatch({ type: 'LoadTopicsStarted' });

            const topics = await apiGetTopics(config);

            store.dispatch({ type: 'LoadTopicsSuccess', topics });
        } catch (ex) {
            store.dispatch({ type: 'LoadTopicsFailed', ex });
        }
    };
}

export function saveProfile(config: SDKConfig, update: UpdateProfileDto) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        try {
            store.dispatch({ type: 'SaveProfileStarted' });

            const profile = await apiPostProfile(config, update);

            store.dispatch({ type: 'SaveProfileSuccess', profile });
        } catch (ex) {
            store.dispatch({ type: 'SaveProfileFailed', ex });
        }
    };
}

export function loadSubscriptions(config: SDKConfig, topics: string[]) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        topics = topics.filter(x => !store.current.subscriptions[x]);

        if (topics.length === 0) {
            return;
        }

        try {
            store.dispatch({ type: 'LoadSubscriptionsStarted', topics });
    
            const subscriptions = await apiGetSubscriptions(config, topics);
    
            store.dispatch({ type: 'LoadSubscriptionsSuccess', topics, subscriptions });
        } catch (ex) {
            store.dispatch({ type: 'LoadSubscriptionsFailed', ex, topics });
        }
    };
}

export function subscribe(config: SDKConfig, subscriptions: SubscriptionsDto) {
    return async (store: Store<NotifoState, NotifoAction>) => {
        if (Object.keys(subscriptions).length === 0) {
            return;
        }

        store.dispatch({ type: 'SubscribeStarted', subscriptions });

        try {
            await apiPostSubscriptions(config, subscriptions);

            store.dispatch({ type: 'SubscribeSuccess', subscriptions });
        } catch (ex) {
            store.dispatch({ type: 'SubscribeFailed', ex, subscriptions });
        }
    };
}

export function setConnected(isConnected: boolean): NotifoAction {
    return { type: 'SetConnected', isConnected };
}

export function deleteNotification(id: string): NotifoAction {
    return { type: 'NotificationRemove', id };
}

export function addNotifications(notifications: ReadonlyArray<NotifoNotificationDto>): NotifoAction {
    return { type: 'NotificationsAdd', notifications };
}
