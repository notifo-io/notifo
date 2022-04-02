/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback, useEffect, useMemo, useRef } from 'preact/hooks';
import { isUndefined, NotificationsOptions, SDKConfig, sendToBoolean, Subscription, Topic, useMutable } from '@sdk/shared';
import { loadSubscriptions, subscribe, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Toggle } from './Toggle';

type Subscriptions = { [path: string]: Subscription | undefined | null };

export interface TopicsViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;
}

export const TopicsView = (props: TopicsViewProps) => {
    const { config } = props;

    const dispatch = useDispatch();
    const loaded = useStore(x => x.topicsLoaded);
    const loading = useStore(x => x.topicsLoading);
    const subscriptions = useStore(x => x.subscriptions);
    const subscriptionsForm = useMutable<Subscriptions>({});
    const subscriptionsValue = subscriptionsForm.get();
    const topics = useStore(x => x.topics);
    const topicsRef = useRef(topics);

    useEffect(() => {
        dispatch(loadSubscriptions(config, topicsRef.current.map(x => x.path)));
    }, [dispatch, config]);

    useEffect(() => {
        const newValue: Subscriptions = {};

        for (const topic of Object.values(topics)) {
            newValue[topic.path] = subscriptions[topic.path]?.subscription;
        }

        subscriptionsForm.set(newValue);
    }, [subscriptionsForm, subscriptions, topics]);

    const loadSubscriptionsInProgress = useMemo(() => {
        return topics.filter(x => subscriptions[x.path]?.loadingStatus === 'InProgress').length > 0;
    }, [subscriptions, topics]);

    const loadSubscriptionsFailed = useMemo(() => {
        return topics.filter(x => subscriptions[x.path]?.loadingStatus === 'Failed').length > 0;
    }, [subscriptions, topics]);

    const saveSubscriptionsInProgress = useMemo(() => {
        return topics.filter(x => subscriptions[x.path]?.updateStatus === 'InProgress').length > 0;
    }, [subscriptions, topics]);

    const saveSubscriptionsFailed = useMemo(() => {
        return topics.filter(x => subscriptions[x.path]?.updateStatus === 'Failed').length > 0;
    }, [subscriptions, topics]);

    const doToggleTopic = useCallback((path: string) => {
        subscriptionsForm.set(value => {
            if (value[path]) {
                value[path] = undefined;
            } else {
                value[path] = { topicSettings: {} };
            }
        });
    }, [subscriptionsForm]);

    const doToggleChannel = useCallback((path: string, channel: string) => {
        subscriptionsForm.set(value => {
            if (value[path]!.topicSettings[channel]?.send === 'Send') {
                value[path]!.topicSettings[channel] = { send: 'DoNotSend' };
            } else {
                value[path]!.topicSettings[channel] = { send: 'Send' };
            }
        });
    }, [subscriptionsForm]);

    const doSave = useCallback(() => {
        dispatch(subscribe(config, subscriptionsValue as any));
    }, [config, dispatch, subscriptionsValue]);

    const disabled = loading === 'InProgress' || loadSubscriptionsInProgress || saveSubscriptionsInProgress;

    return (
        <Fragment>
            {loaded && Object.keys(topics).length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
            }

            <div class='notifo-list-loading'>
                <Loader size={18} visible={loading === 'InProgress' || loadSubscriptionsInProgress} />
            </div>

            {loadSubscriptionsFailed &&
                <div class='notifo-error'>{config.texts.loadingFailed}</div>
            }

            {topics.length > 0 &&
                <Fragment>
                    {Object.values(topics).map(topic => 
                        <TopicRow key={topic}
                            config={config}
                            disabled={disabled}
                            onToggleChannel={doToggleChannel}
                            onToggleTopic={doToggleTopic}
                            subscription={subscriptionsValue[topic.path]}
                            topic={topic} 
                        />,
                    )}

                    <div class='notifo-form-group'>
                        {saveSubscriptionsFailed &&
                            <div class='notifo-error'>{config.texts.savingFailed}</div>
                        }

                        <button class='notifo-form-button primary' type='submit' onClick={doSave} disabled={disabled}>
                            {config.texts.save}
                        </button>

                        <Loader size={16} visible={saveSubscriptionsInProgress} />
                    </div>
                </Fragment>
            }
        </Fragment>
    );
};

const TopicRow = ({ config, disabled, onToggleChannel, onToggleTopic, subscription, topic }: { 
    config: SDKConfig; 
    disabled: boolean;
    onToggleChannel: (topic: string, channel: string) => void;
    onToggleTopic: (path: string) => void;
    subscription?: Subscription | null; 
    topic: Topic;
}) => {
    return (
        <div key={topic} class='notifo-topic'>
            <div class='notifo-topic-toggle'>
                {!isUndefined(subscription) &&
                    <Toggle value={!!subscription} disabled={disabled} 
                        onChange={() => onToggleTopic(topic.path)} />
                }                
            </div>
            <div class='notifo-topic-details'>
                <h3>{topic.name}</h3>

                <div>{topic.description}</div>

                {subscription &&
                    <div>
                        {getAllowedChannels(topic, config).map(channel =>
                            <label key={channel.name} disabled={disabled}>
                                <input type="checkbox" checked={sendToBoolean(subscription.topicSettings[channel.name]?.send)}
                                    onChange={() => onToggleChannel(topic.path, channel.name)} 
                                /> 
                                &nbsp;{channel.label}
                            </label>,
                        )}
                    </div>
                }
            </div>
        </div>
    );
};

function getAllowedChannels(topic: Topic, config: SDKConfig) {
    const result: { name: string; label: string }[] = [];

    for (const [key, value] of Object.entries(topic.channels)) {
        if (value !== 'Allowed' || !config.allowedChannels[key]) {
            continue;
        }

        const label = config.texts[key];

        if (label) {
            result.push({ name: key, label });
        }
    }

    return result;
}