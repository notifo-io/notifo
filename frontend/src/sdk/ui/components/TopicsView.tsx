/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useEffect } from 'preact/hooks';
import { NotificationsOptions, SDKConfig, Topic } from '@sdk/shared';
import { loadTopics, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Toggle } from './Toggle';

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
    const topics = useStore(x => x.topics)!;

    useEffect(() => {
        loadTopics(config, dispatch);
    }, [dispatch, config]);

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
                        <div key={topic} class='notifo-topic'>
                            <div class='notifo-topic-toggle'>
                                <Toggle value={!!topic.subscription} />
                            </div>
                            <div class='notifo-topic-details'>
                                <h3>{topic.name}</h3>

                                <div>{topic.description}</div>

                                <div>
                                    {getAllowedChannels(topic, config).map(channel =>
                                        <label key={channel.name}>
                                            <input type="checkbox" /> {channel.label}
                                        </label>,
                                    )}
                                </div>
                            </div>
                        </div>,
                    )}
                </Fragment>
            }
        </Fragment>
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