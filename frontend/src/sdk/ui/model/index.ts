/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: no-parameter-reassignment

import { apiDeleteSubscription, apiGetProfile, apiGetSubscription, apiPostProfile, apiPostSubscription, NotifoNotification, Profile, SDKConfig, Subscription, UpdateProfile } from '@sdk/shared';
import { useEffect, useState } from 'preact/hooks';
import { Dispatch, set, Store } from './store';

export type Transition = 'InProgress' | 'Failed' | 'Success';

export interface SubscriptionState {
    // The load state.
    transition: Transition;

    // The subscription.
    subscription?: Subscription;
}

export type SubscriptionsState = { [prefix: string]: SubscriptionState };

export interface NotifoState {
    // The current notifications.
    notifications: ReadonlyArray<NotifoNotification>;

    // The notifications state.
    notificationsTransition: Transition;

    // The profile state.
    profileTransition?: Transition;

    // The loaded profile
    profile?: Profile;

    // True when connected.
    isConnected?: boolean;

    // The subscriptions.
    subscriptions: SubscriptionsState;
}

type NotifoDispatch = Dispatch<NotifoAction>;
type NotifoAction = { type: NotifoActionType, [x: string]: any  };

type NotifoActionType =
    'Connected' |
    'Disconnected' |
    'LoadProfileFailed' |
    'LoadProfileStarted' |
    'LoadProfileSuccess' |
    'LoadSubscriptionFailed' |
    'LoadSubscriptionStarted' |
    'LoadSubscriptionSuccess' |
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
        case 'SetConnected': {
            return { ...state, isConnected: action.isConnected };
        }
        case 'LoadProfileStarted': {
            return { ...state, profileTransition: 'InProgress' };
        }
        case 'LoadProfileFailed': {
            return { ...state, profileTransition: 'Failed' };
        }
        case 'LoadProfileSuccess': {
            return { ...state, profileTransition: 'Success', profile: action.profile };
        }
        case 'SaveProfileStarted': {
            return { ...state, profileTransition: 'InProgress' };
        }
        case 'SaveProfileFailed': {
            return { ...state, profileTransition: 'Failed' };
        }
        case 'SaveProfileSuccess': {
            return { ...state, profileTransition: 'Success', profile: action.profile };
        }
        case 'LoadSubscriptionStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'LoadSubscriptionSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case 'SubscribeStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'SubscribeFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'SubscribeSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case 'UnsubscribeStarted': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'UnsubscribeFailed': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case 'UnsubscribeSuccess': {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: null });

            return { ...state, subscriptions };
        }
        case 'NotificationRemove': {
            const notifications = state.notifications.filter(x => x.id !== action.id);

            return { ...state, notifications };
        }
        case 'NotificationsAdd': {
            const newNotifications: ReadonlyArray<NotifoNotification> = action.notifications;

            if (newNotifications.length === 0) {
                if (state.notificationsTransition === 'Success') {
                    return state;
                } else {
                    return { ...state, notificationsTransition: 'Success' };
                }
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
                const x = b.created;
                const y = a.created;

                return x > y ? 1 : x < y ? -1 : 0;
            });

            return { ...state, notifications, notificationsTransition: 'Success' };
        }
    }

    return state;
}

const initialState: NotifoState = {
    isConnected: false,
    notifications: [],
    notificationsTransition: 'InProgress',
    profile: undefined,
    profileTransition: undefined,
    subscriptions: {},
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
    }, []);

    return state;
}

export function getUnseen(state: NotifoState) {
    let count = 0;

    for (const notification of state.notifications) {
        if (notification.trackingUrl && !notification.isSeen) {
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

        subscription = await apiPostSubscription(config, { topicPrefix, ...subscription });

        dispatch({ type: 'SubscribeSuccess', topicPrefix, subscription });
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
