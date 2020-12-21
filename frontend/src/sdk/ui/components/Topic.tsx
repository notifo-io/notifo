/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useCallback, useEffect } from 'preact/hooks';
import { SDKConfig, TopicOptions } from './../../shared';
import { loadSubscription, subscribe, unsubscribe, useNotifoState } from './../model';
import { TopicButton } from './TopicButton';

export interface TopicProps {
    config: SDKConfig;

    // The topic options.
    options: TopicOptions;

    // The topic to watch.
    topic: string;
}

export const TopicContainer = (props: TopicProps) => {
    const { config, options, topic } = props;

    const [state, dispatch] = useNotifoState();
    const subscription = state.subscriptions[topic] || 'Unknown';

    useEffect(() => {
        if (subscription === 'Unknown') {
            loadSubscription(config, topic, dispatch);
        }
    }, []);

    const doToggle = useCallback(() => {
        if (subscription === 'Subscribed') {
            unsubscribe(config, topic, dispatch);
        } else if (subscription === 'NotSubscribed') {
            subscribe(config, topic, dispatch);
        }
    }, [subscription]);

    return (
        <div className='notifo'>
            <TopicButton options={options} subscription={subscription} onClick={doToggle} />
        </div>
    );
};
