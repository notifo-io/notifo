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
import { loadSubscription, useDispatch, useStore } from './../model';
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

    const dispatch = useDispatch();
    const subscription = useStore(x => x.subscriptions[topic]?.subscription);
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        if (subscription === undefined) {
            loadSubscription(config, topic, dispatch);
        }
    }, []);

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    const doHide = useCallback(() => {
        setIsOpen(false);
    }, []);

    return (
        <div className='notifo'>
            <TopicButton options={options} subscription={subscription} onClick={doShow} />

            {isOpen &&
                <TopicModal config={config} options={options}
                    onClickOutside={doHide} />
            }
        </div>
    );
};
