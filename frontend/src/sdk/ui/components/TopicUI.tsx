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
    topicPrefix: string;
}

export const TopicUI = (props: TopicUIProps) => {
    const {
        config,
        options,
        topicPrefix,
    } = props;

    const dispatch = useDispatch();
    const subscriptionState = useStore(x => x.subscriptions[topicPrefix]);
    const subscription = subscriptionState?.subscription;
    const subscriptionStatus = subscriptionState?.status;
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        if (isUndefined(subscription)) {
            loadSubscriptions(config, [topicPrefix], dispatch);
        }
    }, [dispatch, config, subscription, topicPrefix]);

    const doShow = useCallback(() => {
        setIsOpen(true);
    }, []);

    const doHide = useCallback(() => {
        setIsOpen(false);
    }, []);

    if (isUndefined(subscription)) {
        return null;
    }

    return (
        <div class='notifo'>
            <TopicButton options={options} subscription={subscription} onClick={doShow} />

            {isOpen &&
                <TopicModal config={config} options={options}
                    subscription={subscription}
                    subscriptionState={subscriptionStatus}
                    topicPrefix={topicPrefix}
                    onClickOutside={doHide} />
            }
        </div>
    );
};
