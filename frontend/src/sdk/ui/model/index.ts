/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: no-parameter-reassignment

import { useEffect, useState } from 'preact/hooks';
import { apiDeleteSubscription, apiGetArchive, apiGetProfile, apiGetSubscription, apiGetTopics, apiPostProfile, apiPostSubscription, NotifoNotification, Profile, SDKConfig, Subscription, Topic, UpdateProfile } from '@sdk/shared';
import { Dispatch, set, Store } from './store';

export type Status = 'InProgress' | 'Failed' | 'Success';

export interface SubscriptionState {
    // The load state.
    status: Status;

    // The subscription.
    subscription?: Subscription | null;
}

export type SubscriptionsState = { [prefix: string]: SubscriptionState };

export interface NotifoState {
    // The current notifications.
    archive: ReadonlyArray<NotifoNotification>;

    // The notifications state.
    archiveStatus: Status;

    // The current notifications.
    notifications: ReadonlyArray<NotifoNotification>;

    // The notifications state.
    notificationsStatus: Status;

    // The profile status.
    profileStatus?: Status;

    // The loaded profile
    profile?: Profile;

    // True when connected.
    isConnected?: boolean;

    // The subscriptions.
    subscriptions: SubscriptionsState;

    // The topics.
    topicsStatus?: Status;

    // The loaded topics.
    topics: { [path: string]: Topic };
}

type NotifoDispatch = Dispatch<NotifoAction>;
type NotifoAction = { type: NotifoActionType; [x: string]: any };

type NotifoActionType =
    'Connected' |
    'Disconnected' |
    'LoadProfileFailed' |
    'LoadProfileStarted' |
    'LoadProfileSuccess' |
    'LoadSubscriptionFailed' |
    'LoadSubscriptionStarted' |
    'LoadSubscriptionSuccess' |
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
    'SubscribeSuccess' |
    'UnsubscribeFailed' |
    'UnsubscribeStarted' |
    'UnsubscribeSuccess';

function reducer(state: NotifoState, action: NotifoAction): NotifoState {
    switch (action.type) {
        case 'SetConnected':
            return { ...state, isConnected: action.isConnected };
        case 'LoadProfileStarted':
            return { ...state, profileStatus: 'InProgress' };
        case 'LoadProfileFailed':
            return { ...state, profileStatus: 'Failed' };
        case 'LoadProfileSuccess':
            return { ...state, profileStatus: 'Success', profile: action.profile };
        case 'SaveProfileStarted':
            return { ...state, profileStatus: 'InProgress' };
        case 'SaveProfileFailed':
            return { ...state, profileStatus: 'Failed' };
        case 'SaveProfileSuccess':
            return { ...state, profileStatus: 'Success', profile: action.profile };
        case 'LoadArchiveStarted':
            return { ...state, archiveStatus: 'InProgress' };
        case 'LoadArchiveFailed':
            return { ...state, archiveStatus: 'Failed' };
        case 'LoadArchiveSuccess':
            return { ...state, archiveStatus: 'Success', archive: action.notifications };
        case 'LoadTopicsStarted':
            return { ...state, topicsStatus: 'InProgress' };
        case 'LoadTopicsFailed':
            return { ...state, topicsStatus: 'Failed' };
        case 'LoadTopicsSuccess': {
            const topics: { [path: string]: Topic } = {};

            let subscriptions = state.subscriptions;

            for (const topic of action.topics as Topic[]) {
                topics[topic.path] = topic;

                if (!topic.subscription) {
                    continue;
                }

                subscriptions =
                    set(subscriptions, topic.path,
                        { status: 'Success', subscription: topic.subscription });
            }

            return { ...state, subscriptions, topicsStatus: 'Success', topics };
        }
        case 'LoadSubscriptionStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { status: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case 'SubscribeStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'SubscribeFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'SubscribeSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { status: 'Success', subscription: action.subscription });

            const topics =
                set(state.topics, action.topixPrefix, 
                    t => ({ ...t, subscroption: action.subscription.topicSettings }));

            return { ...state, subscriptions, topics };
        }
        case 'UnsubscribeStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'UnsubscribeFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, status: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'UnsubscribeSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { status: 'Success', subscription: null });

            const topics =
                set(state.topics, action.topixPrefix, 
                    t => ({ ...t, subscription: undefined }));

            return { ...state, subscriptions, topics };
        }
        case 'NotificationRemove': {
            const notifications = state.notifications.filter(x => x.id !== action.id);

            return { ...state, notifications };
        }
        case 'NotificationsAdd': {
            const newNotifications: ReadonlyArray<NotifoNotification> = action.notifications;

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
    isConnected: false,
    archive: [],
    archiveStatus: 'InProgress',
    notifications: [],
    notificationsStatus: 'InProgress',
    profile: undefined,
    profileStatus: undefined,
    subscriptions: {},
    topics: {},
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

export async function loadSubscription(config: SDKConfig, topicPrefix: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'LoadSubscriptionStarted', topicPrefix });

        const subscription = await apiGetSubscription(config, topicPrefix);

        dispatch({ type: 'LoadSubscriptionSuccess', topicPrefix, subscription });
    } catch (ex) {
        dispatch({ type: 'LoadSubscriptionFailed', ex, topicPrefix });
    }
}

export async function subscribe(config: SDKConfig, topicPrefix: string, subscription: Subscription, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'SubscribeStarted', topicPrefix });

        const newSubscription = await apiPostSubscription(config, { topicPrefix, ...subscription });

        dispatch({ type: 'SubscribeSuccess', topicPrefix, subscription: newSubscription });
    } catch (ex) {
        dispatch({ type: 'SubscribeFailed', ex, topicPrefix });
    }
}

export async function unsubscribe(config: SDKConfig, topicPrefix: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'UnsubscribeStarted', topicPrefix });

        await apiDeleteSubscription(config, topicPrefix);

        dispatch({ type: 'UnsubscribeSuccess', topicPrefix });
    } catch (ex) {
        dispatch({ type: 'UnsubscribeFailed', ex, topicPrefix });
    }
}

export async function loadProfile(config: SDKConfig, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'LoadProfileStarted' });

        const profile = await apiGetProfile(config);

        dispatch({ type: 'LoadProfileSuccess', profile });
    } catch (ex) {
        dispatch({ type: 'LoadProfileFailed', ex });
    }
}

export async function loadArchive(config: SDKConfig, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'LoadArchiveStarted' });

        const notifications = await apiGetArchive(config);

        dispatch({ type: 'LoadArchiveSuccess', notifications });
    } catch (ex) {
        dispatch({ type: 'LoadArchiveFailed', ex });
    }
}

export async function loadTopics(config: SDKConfig, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'LoadTopicsStarted' });

        const topics = await apiGetTopics(config);

        dispatch({ type: 'LoadTopicsSuccess', topics });
    } catch (ex) {
        dispatch({ type: 'LoadTopicsFailed', ex });
    }
}

export async function saveProfile(config: SDKConfig, update: UpdateProfile, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: 'SaveProfileStarted' });

        const profile = await apiPostProfile(config, update);

        dispatch({ type: 'SaveProfileSuccess', profile });
    } catch (ex) {
        dispatch({ type: 'SaveProfileFailed', ex });
    }
}

export function setConnected(isConnected: boolean, dispatch: (action: any) => void) {
    dispatch({ type: 'SetConnected', isConnected });
}

export function deleteNotification(id: string, dispatch: (action: any) => void) {
    dispatch({ type: 'NotificationRemove', id });
}

export function addNotifications(notifications: ReadonlyArray<NotifoNotification>, dispatch: (action: any) => void) {
    dispatch({ type: 'NotificationsAdd', notifications });
}
