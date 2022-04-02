/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { isUndefined, SDKConfig, TopicOptions } from '@sdk/shared';
import { loadSubscriptions, useDispatch, useStore } from '@sdk/ui/model';
import { TopicButton } from './TopicButton';
import { TopicModal } from './TopicModal';

export interface TopicUIProps {
    config: SDKConfig;

    // The topic options.
    options: TopicOptions;

    // The topic to watch.
    topic: string;
}

export const TopicUI = (props: TopicUIProps) => {
    const {
        config,
        options,
        topic,
    } = props;

    const dispatch = useDispatch();
    const subscriptionState = useStore(x => x.subscriptions[topic]);
    const subscriptionStatus = subscriptionState?.updateStatus;
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        dispatch(loadSubscriptions(config, [topic]));
    }, [dispatch, config, topic]);

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    const doHide = useCallback(() => {
        setIsOpen(false);
    }, []);

    if (isUndefined(subscriptionState?.subscription)) {
        return null;
    }

    return (
        <div class='notifo'>
            <TopicButton options={options} subscription={subscriptionState?.subscription} onClick={doShow} />

            {isOpen &&
                <TopicModal config={config} options={options}
                    subscription={subscriptionState?.subscription}
                    subscriptionStatus={subscriptionStatus}
                    topic={topic}
                    onClickOutside={doHide} />
            }
        </div>
    );
};
