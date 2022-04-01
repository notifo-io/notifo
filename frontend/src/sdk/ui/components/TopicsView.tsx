/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback, useEffect, useMemo } from 'preact/hooks';
import { NotificationsOptions, SDKConfig, sendToBoolean, Subscription, Topic, useMutable } from '@sdk/shared';
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
    const isLoaded = useStore(x => x.topicsStatus !== 'InProgress');
    const isLoading = useStore(x => x.topicsStatus === 'InProgress');
    const subscriptions = useStore(x => x.subscriptions);
    const subscriptionsForm = useMutable<Subscriptions>({});
    const subscriptionsValue = subscriptionsForm.get();
    const topics = useStore(x => x.topics)!;

    useEffect(() => {
        loadSubscriptions(config, topics.map(x => x.path), dispatch);
    }, [dispatch, config, topics]);

    useEffect(() => {
        const newValue: Subscriptions = {};

        for (const topic of Object.values(topics)) {
            newValue[topic.path] = subscriptions[topic.path]?.subscription;
        }

        subscriptionsForm.set(newValue);
    }, [subscriptionsForm, subscriptions, topics]);

    const isSaving = useMemo(() => {
        return topics.filter(x => subscriptions[x.path]?.status === 'InProgress').length > 0;
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
        subscribe(config, subscriptionsValue as any, dispatch);
    }, [config, dispatch, subscriptionsValue]);

    return (
        <Fragment>
            {isLoading && Object.keys(topics).length === 0 &&
                <div class='notifo-list-loading'>
                    <Loader size={18} visible={true} />
                </div>
            }

            {isLoaded && Object.keys(topics).length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
            }

            {isLoaded && Object.keys(topics).length > 0 &&
                <Fragment>
                    {Object.values(topics).map(topic => 
                        <TopicRow key={topic}
                            config={config}
                            disabled={isSaving}
                            onToggleChannel={doToggleChannel}
                            onToggleTopic={doToggleTopic}
                            subscription={subscriptionsValue[topic.path]}
                            topic={topic} 
                        />,
                    )}

                    <div class='notifo-form-group'>
                        <button class='notifo-form-button primary' type='submit' onClick={doSave} disabled={isSaving}>
                            {config.texts.save}
                        </button>

                        <Loader size={16} visible={isSaving} />
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
                <Toggle value={!!subscription} disabled={disabled}
                    onChange={() => onToggleTopic(topic.path)} />
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