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

enum NotifoActionType {
    AddNotifications,
    Connected,
    Disconnected,
    LoadSubscriptionFailed,
    LoadSubscriptionStarted,
    LoadSubscriptionSuccess,
    SubscribeFailed,
    SubscribeStarted,
    SubscribeSuccess,
    UnsubscribeFailed,
    UnsubscribeStarted,
    UnsubscribeSuccess,
    LoadProfileFailed,
    LoadProfileStarted,
    LoadProfileSuccess,
    SaveProfileFailed,
    SaveProfileStarted,
    SaveProfileSuccess,
    SetConnected,
}

function reducer(state: NotifoState, action: NotifoAction): NotifoState {
    switch (action.type) {
        case NotifoActionType.SetConnected: {
            return { ...state, isConnected: action.isConnected };
        }
        case NotifoActionType.LoadProfileStarted: {
            return { ...state, profileTransition: 'InProgress' };
        }
        case NotifoActionType.LoadProfileFailed: {
            return { ...state, profileTransition: 'Failed' };
        }
        case NotifoActionType.LoadProfileSuccess: {
            return { ...state, profileTransition: 'Success', profile: action.profile };
        }
        case NotifoActionType.SaveProfileStarted: {
            return { ...state, profileTransition: 'InProgress' };
        }
        case NotifoActionType.SaveProfileFailed: {
            return { ...state, profileTransition: 'Failed' };
        }
        case NotifoActionType.SaveProfileSuccess: {
            return { ...state, profileTransition: 'Success', profile: action.profile };
        }
        case NotifoActionType.LoadSubscriptionStarted: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionFailed: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeStarted: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeFailed: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeStarted: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'InProgress' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeFailed: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    s => ({ ...s, transition: 'Failed' } as SubscriptionState));

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topicPrefix,
                    { transition: 'Success', subscription: null });

            return { ...state, subscriptions };
        }
        case NotifoActionType.AddNotifications: {
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
        dispatch({ type: NotifoActionType.LoadSubscriptionStarted, topicPrefix });

        const subscription = await apiGetSubscription(config, topicPrefix);

        dispatch({ type: NotifoActionType.LoadSubscriptionSuccess, topicPrefix, subscription });
    } catch (ex) {
        dispatch({ type: NotifoActionType.LoadSubscriptionFailed, ex, topicPrefix });
    }
}

export async function subscribe(config: SDKConfig, topicPrefix: string, subscription: Subscription, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.SubscribeStarted, topicPrefix });

        subscription = await apiPostSubscription(config, { topicPrefix, ...subscription });

        dispatch({ type: NotifoActionType.SubscribeSuccess, topicPrefix, subscription });
    } catch (ex) {
        dispatch({ type: NotifoActionType.SubscribeFailed, ex, topicPrefix });
    }
}

export async function unsubscribe(config: SDKConfig, topicPrefix: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.UnsubscribeStarted, topicPrefix });

        await apiDeleteSubscription(config, topicPrefix);

        dispatch({ type: NotifoActionType.UnsubscribeSuccess, topicPrefix });
    } catch (ex) {
        dispatch({ type: NotifoActionType.UnsubscribeFailed, ex, topicPrefix });
    }
}

export async function loadProfile(config: SDKConfig, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.LoadProfileStarted });

        const profile = await apiGetProfile(config);

        dispatch({ type: NotifoActionType.LoadProfileSuccess, profile });
    } catch (ex) {
        dispatch({ type: NotifoActionType.LoadProfileFailed, ex });
    }
}

export async function saveProfile(config: SDKConfig, update: UpdateProfile, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.SaveProfileStarted });

        const profile = await apiPostProfile(config, update);

        dispatch({ type: NotifoActionType.SaveProfileSuccess, profile });
    } catch (ex) {
        dispatch({ type: NotifoActionType.SaveProfileFailed, ex });
    }
}

export function setConnected(isConnected: boolean, dispatch: (action: any) => void) {
    dispatch({ type: NotifoActionType.SetConnected, isConnected });
}

export function addNotifications(notifications: ReadonlyArray<NotifoNotification>, dispatch: (action: any) => void) {
    dispatch({ type: NotifoActionType.AddNotifications, notifications });
}
