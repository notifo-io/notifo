/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { isUndefined, SDKConfig, sendToBoolean, SubscriptionDto, TopicDto } from '@sdk/shared';
import { Toggle } from './Toggle';

export interface TopicItemProps {
    // The main config.
    config: SDKConfig;
    
    // True, when the form is disabled.
    disabled?: boolean;

    // The assigned subscription.
    subscription?: SubscriptionDto | null;

    // The current topic.
    topic: TopicDto;

    // Trigger when the channel setting has been changed.
    onChangeChannel: (send: boolean | undefined, topic: string, channel: string) => void;

    // Invoked when the topic setting has beenchanged.
    onChangeTopic: (send: boolean | undefined, path: string) => void;
}

export const TopicItem = (props: TopicItemProps) => {
    const {
        config,
        disabled,
        onChangeChannel,
        onChangeTopic,
        subscription,
        topic,
    } = props;

    return (
        <div key={topic} class='notifo-topic'>
            <div class='notifo-topic-toggle'>
                {!isUndefined(subscription) &&
                    <Toggle value={!!subscription} disabled={disabled} name={topic.path}
                        onChange={onChangeTopic} />
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
                                    onChange={event => onChangeChannel(event.currentTarget.checked, topic.path, channel.name)} 
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

function getAllowedChannels(topic: TopicDto, config: SDKConfig) {
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