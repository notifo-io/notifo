/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { useEffect, useState } from 'preact/hooks';
import { apiUpdateSubscription, NotifoNotification } from './../../api';
import { SDKConfig } from './../../shared';
import { Dispatch, set, Store } from './store';

export type TopicState = 'Pending' | 'Subscribed' | 'NotSubscribed' | 'Unknown';
export type TopicsState = { [prefix: string]: TopicState };

export type NotifoState = { notifications: NotifoNotification[], subscriptions: TopicsState };

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
}

function reducer(state: NotifoState, action: NotifoAction) {
    switch (action.type) {
        case NotifoActionType.LoadSubscriptionStarted: {
            const subscriptions = set(state.subscriptions, action.topic, 'Pending');

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionFailed: {
            const subscriptions = set(state.subscriptions, action.topic, 'Unknown');

            return { ...state, subscriptions };
        }
        case NotifoActionType.LoadSubscriptionSuccess: {
            const subscriptions = set(state.subscriptions, action.topic, action.found ? 'Subscribed' : 'NotSubscribed');

            return { ...state, subscriptions };
        }
        case NotifoActionType.SubscribeStarted: {
            const subscriptions = set(state.subscriptions, action.topic, 'Subscribed');

            return { ...state, subscriptions };
        }
        case NotifoActionType.UnsubscribeStarted: {
            const subscriptions = set(state.subscriptions, action.topic, 'NotSubscribed');

            return { ...state, subscriptions };
        }
        case NotifoActionType.AddNotifications: {
            const newNotifications: ReadonlyArray<NotifoNotification> = action.notifications;

            if (newNotifications.length === 0) {
                return state;
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

            return { ...state, notifications };
        }
    }

    return state;
}

const initialState: NotifoState = {
    notifications: [], subscriptions: {},
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

        const found = await apiUpdateSubscription(config, 'GET', topic);

        dispatch({ type: NotifoActionType.LoadSubscriptionSuccess, topic, found });
    } catch (ex) {
        dispatch({ type: NotifoActionType.LoadSubscriptionFailed, ex, topic });
    }
}

export async function subscribe(config: SDKConfig, topic: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.SubscribeStarted, topic });

        await apiUpdateSubscription(config, 'POST', topic);

        dispatch({ type: NotifoActionType.SubscribeSuccess, topic });
    } catch (ex) {
        dispatch({ type: NotifoActionType.SubscribeFailed, ex, topic });
    }
}

export async function unsubscribe(config: SDKConfig, topic: string, dispatch: NotifoDispatch) {
    try {
        dispatch({ type: NotifoActionType.UnsubscribeStarted, topic });

        await apiUpdateSubscription(config, 'DELETE', topic);

        dispatch({ type: NotifoActionType.UnsubscribeSuccess, topic });
    } catch (ex) {
        dispatch({ type: NotifoActionType.UnsubscribeFailed, ex, topic });
    }
}

export function addNotifications(notifications: ReadonlyArray<NotifoNotification>, dispatch: (action: any) => void) {
    dispatch({ type: NotifoActionType.AddNotifications, notifications });
}
