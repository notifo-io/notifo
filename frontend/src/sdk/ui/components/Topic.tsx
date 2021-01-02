/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useCallback, useEffect, useState } from 'preact/hooks';
import { SDKConfig, TopicOptions } from './../../shared';
import { loadSubscription, useNotifoState } from './../model';
import { TopicButton } from './TopicButton';
import { TopicModal } from './TopicModal';

export interface TopicProps {
    config: SDKConfig;

    // The topic options.
    options: TopicOptions;

    // The topic to watch.
    topic: string;
}

export const TopicContainer = (props: TopicProps) => {
    const {
        config,
        options,
        topic,
    } = props;

    const [state, dispatch] = useNotifoState();
    const [isOpen, setIsOpen] = useState(false);
    const subscription = state.subscriptions[topic]?.subscription;

    useEffect(() => {
        if (subscription === undefined) {
            loadSubscription(config, topic, dispatch);
        }
    }, []);

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    return (
        <div className='notifo'>
            <TopicButton options={options} subscription={subscription} onClick={doShow} />

            {isOpen &&
                <TopicModal config={config} options={options} />
            }
        </div>
    );
};
