/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { booleanToSend, SDKConfig, sendToBoolean, Subscription, TopicOptions } from '@sdk/shared';
import { subscribe, Status, unsubscribe, useDispatch } from '@sdk/ui/model';
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
    const [subscriptionToEdit, setSubscriptionToEdit] = useState<Subscription | null>(null);

    useEffect(() => {
        setSubscriptionToEdit(subscription || { topicSettings: {} });
    }, [subscription]);

    const doSetEmail = useCallback((send: boolean | undefined) => {
        if (subscriptionToEdit) {
            setChannel(subscriptionToEdit, 'email', send);
        }
    }, [subscriptionToEdit]);

    const doSetPush = useCallback((send: boolean | undefined) => {
        if (subscriptionToEdit) {
            setChannel(subscriptionToEdit, 'webpush', send);
        }
    }, [subscriptionToEdit]);

    const doSubscribe = useCallback(() => {
        if (subscriptionToEdit) {
            subscribe(config, topicPrefix, subscriptionToEdit, dispatch);
        }
    }, [dispatch, config, subscriptionToEdit, topicPrefix]);

    const doUnsubscribe = useCallback(() => {
        unsubscribe(config, topicPrefix, dispatch);
    }, [dispatch, config, topicPrefix]);

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            {subscriptionToEdit &&
                <Fragment>
                    {config.allowEmails &&
                        <div class='notifo-form-group'>
                            <Toggle indeterminate value={sendToBoolean(subscriptionToEdit.topicSettings?.email?.send)}
                                onChange={doSetEmail} />

                            <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                        </div>
                    }

                    <div class='notifo-form-group'>
                        <Toggle indeterminate value={sendToBoolean(subscriptionToEdit.topicSettings?.webpush?.send)}
                            onChange={doSetPush} />

                        <label class='notifo-form-toggle-label'>{config.texts.notifyBeWebPush}</label>
                    </div>

                    <hr />

                    <div class='notifo-form-group'>
                        <button class='notifo-form-button primary' onClick={doSubscribe}>
                            {subscription ? (
                                <span>{config.texts.save}</span>
                            ) : (
                                <span>{config.texts.subscribe}</span>
                            )}
                        </button>

                        {subscription &&
                            <button class='notifo-form-button' onClick={doUnsubscribe}>
                                {config.texts.unsubscribe}
                            </button>
                        }

                        <Loader size={16} visible={subscriptionState === 'InProgress'} />
                    </div>
                </Fragment>
            }
        </Modal>
    );
};

function setChannel(subscription: Subscription, channel: string, value?: boolean) {
    if (!subscription.topicSettings) {
        subscription.topicSettings = {};
    }

    const send = booleanToSend(value);

    if (!subscription.topicSettings[channel]) {
        subscription.topicSettings[channel] = { send };
    } else {
        subscription.topicSettings[channel].send = send;
    }
}
