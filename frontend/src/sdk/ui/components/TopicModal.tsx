/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback, useEffect } from 'preact/hooks';
import { booleanToSend, SDKConfig, sendToBoolean, Subscription, TopicOptions, useMutable } from '@sdk/shared';
import { Status, subscribe, useDispatch } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Modal } from './Modal';
import { Toggle } from './Toggle';

export interface TopicModalProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: TopicOptions;

    // The subscription.
    subscription: Subscription | null;

    // The subscription.
    subscriptionState: Status;

    // The topic to watch.
    topicPrefix: string;

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

export const TopicModal = (props: TopicModalProps) => {
    const {
        config,
        onClickOutside,
        options,
        subscription,
        subscriptionState,
        topicPrefix,
    } = props;

    const dispatch = useDispatch();
    const subscriptionForm = useMutable<Subscription>({ topicSettings: {} });
    const subscriptionValue = subscriptionForm.get();
    const inProgress = subscriptionState === 'InProgress';

    useEffect(() => {
        subscriptionForm.set(subscription || { topicSettings: {} });
    }, [subscription, subscriptionForm]);

    const doSetEmail = useCallback((send: boolean | undefined) => {
        subscriptionForm.set(v => setChannel(v, 'email', send));
    }, [subscriptionForm]);

    const doSetPush = useCallback((send: boolean | undefined) => {
        subscriptionForm.set(v => setChannel(v, 'email', send));
    }, [subscriptionForm]);

    const doSubscribe = useCallback(() => {
        subscribe(config, { [topicPrefix]: subscriptionValue }, dispatch);
    }, [dispatch, config, subscriptionValue, topicPrefix]);

    const doUnsubscribe = useCallback(() => {
        subscribe(config, { [topicPrefix]: null }, dispatch);
    }, [dispatch, config, topicPrefix]);

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            <Fragment>
                {config.allowedChannels['email'] &&
                    <div class='notifo-form-group'>
                        <Toggle indeterminate value={sendToBoolean(subscriptionValue.topicSettings?.email?.send)} disabled={inProgress}
                            onChange={doSetEmail} />

                        <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                    </div>
                }

                <div class='notifo-form-group'>
                    <Toggle indeterminate value={sendToBoolean(subscriptionValue.topicSettings?.webpush?.send)} disabled={inProgress}
                        onChange={doSetPush} />

                    <label class='notifo-form-toggle-label'>{config.texts.notifyBeWebPush}</label>
                </div>
                
                <hr />

                <div class='notifo-form-group'>
                    <button class='notifo-form-button primary' onClick={doSubscribe} disabled={inProgress}>
                        {subscription ? (
                            <span>{config.texts.save}</span>
                        ) : (
                            <span>{config.texts.subscribe}</span>
                        )}
                    </button>

                    {subscription &&
                        <button class='notifo-form-button' onClick={doUnsubscribe} disabled={inProgress}>
                            {config.texts.unsubscribe}
                        </button>
                    }

                    <Loader size={16} visible={subscriptionState === 'InProgress'} />
                </div>
            </Fragment>
        </Modal>
    );
};

function setChannel(subscription: Subscription, channel: string, value?: boolean) {
    const send = booleanToSend(value);

    if (!subscription.topicSettings[channel]) {
        subscription.topicSettings[channel] = { send };
    } else {
        subscription.topicSettings[channel].send = send;
    }
}
