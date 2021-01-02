/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: no-parameter-reassignment

import { useEffect, useState } from 'preact/hooks';
import { apiDeleteSubscription, apiGetProfile, apiGetSubscription, apiPostSubscription, NotifoNotification, Profile, Subscription } from './../../api';
import { SDKConfig } from './../../shared';
import { Dispatch, set, Store } from './store';

export type SubscriptionState = {
    // The load state.
    transition: 'InProgress' | 'Failed' | 'Success',

    // The subscription.
    subscription?: Subscription,
};

export type SubscriptionsState = { [prefix: string]: SubscriptionState };

export type NotifoState = {
    // The current notifications.
    notifications: ReadonlyArray<NotifoNotification>,

    // The notifications state.
    notificationsTransition: 'InProgress' | 'Loaded';

    // The profile state.
    profileTransition?: 'InProgress' | 'Failed' | 'Success',

    // The loaded profile
    profile?: Profile,

    // True when connected.
    isConnected?: boolean,

    // The subscriptions.
    subscriptions: SubscriptionsState;
};

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

function reducer(state: NotifoState, action: NotifoAction) {
    switch (action.type) {
        case NotifoActionType.SetConnected: {
            return { ...state, isConnected: action.isConnected };
        }
        case NotifoActionType.LoadSubscriptionStarted: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'InProgress' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionFailed: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'Failed' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeStarted: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'InProgress' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeFailed: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'Failed' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    { transition: 'Success', subscription: action.subscription });

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeStarted: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'InProgress' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeFailed: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    s => ({ ...s, transition: 'Failed' } as any));

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeSuccess: {
            const subscriptions =
                set(state.subscriptions, action.topic,
                    { transition: 'Success', subscription: null });

            return { ...state, subscriptions };
        }
        case NotifoActionType.AddNotifications: {
            const newNotifications: ReadonlyArray<NotifoNotification> = action.notifications;

            if (newNotifications.length === 0) {
                if (state.notificationsTransition === 'Loaded') {
                    return state;
                } else {
                    return { ...state, notificationsTransition: 'Loaded' } as any;
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

            return { ...state, notifications, notificationsTransition: 'Loaded' };
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

export function useNotifoState(): [NotifoState, NotifoDispatch, Store<NotifoState, NotifoAction>] {
    const [state, setState] = useState(store.current);

    useEffect(() => {
        const listener = (newState: NotifoState) => {
            setState(newState);
        };

        store.subscribe(listener);

        return () => {
            store.unsubscribe(listener);
        };
    }, []);

    return [state, store.dispatch, store];
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

export async function loadSubscription(config: SDKConfig, topic: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.LoadSubscriptionStarted, topic });

        const subscription = await apiGetSubscription(config, topic);

        dispatch({ type: NotifoActionType.LoadSubscriptionSuccess, topic, subscription });
    } catch (ex) {
        dispatch({ type: NotifoActionType.LoadSubscriptionFailed, ex, topic });
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

export async function subscribe(config: SDKConfig, subscription: Subscription, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.SubscribeStarted, subscription });

        subscription = await apiPostSubscription(config, subscription);

        dispatch({ type: NotifoActionType.SubscribeSuccess, subscription });
    } catch (ex) {
        dispatch({ type: NotifoActionType.SubscribeFailed, ex, subscription });
    }
}

export async function unsubscribe(config: SDKConfig, subscription: Subscription, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.UnsubscribeStarted, subscription });

        await apiDeleteSubscription(config, subscription);

        dispatch({ type: NotifoActionType.UnsubscribeSuccess, subscription });
    } catch (ex) {
        dispatch({ type: NotifoActionType.UnsubscribeFailed, ex, subscription });
    }
}

export function setConnected(isConnected: boolean, dispatch: (action: any) => void) {
    dispatch({ type: NotifoActionType.SetConnected, isConnected });
}

export function addNotifications(notifications: ReadonlyArray<NotifoNotification>, dispatch: (action: any) => void) {
    dispatch({ type: NotifoActionType.AddNotifications, notifications });
}
