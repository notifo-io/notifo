/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { useEffect } from 'preact/hooks';
import { isUndefined, SDKConfig, TopicOptions } from '@sdk/shared';
import { loadSubscriptions, useDispatch, useStore } from '@sdk/ui/model';
import { Modal } from './Modal';
import { TopicButton } from './TopicButton';
import { TopicModal } from './TopicModal';
import { useToggle } from './utils';

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
    const modal = useToggle();
    const subscriptionState = useStore(x => x.subscriptions[topic]);
    const subscriptionStatus = subscriptionState?.updateStatus;

    useEffect(() => {
        dispatch(loadSubscriptions(config, [topic]));
    }, [dispatch, config, topic]);

    if (isUndefined(subscriptionState?.subscription)) {
        return null;
    }

    return (
        <div class='notifo'>
            <TopicButton options={options} subscription={subscriptionState?.subscription} onClick={modal.show} />

            {modal.isOpen &&
                <Modal onClickOutside={modal.hide} position={options.position}>
                    <TopicModal
                        config={config}
                        subscription={subscriptionState?.subscription}
                        subscriptionStatus={subscriptionStatus}
                        topic={topic}
                    />
                </Modal>
            }
        </div>
    );
};
