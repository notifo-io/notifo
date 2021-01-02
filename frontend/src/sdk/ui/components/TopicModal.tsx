/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { SDKConfig, Subscription, TopicOptions } from '@sdk/shared';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { Modal } from './Modal';
import { Toggle } from './Toggle';

export interface TopicModalProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: TopicOptions;

    // The subscription.
    subscription?: Subscription;

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

export const TopicModal = (props: TopicModalProps) => {
    const {
        config,
        onClickOutside,
        options,
        subscription,
    } = props;

    const [subscriptionToEdit, setSubscriptionToEdit] = useState<Subscription>({ settings: {} });

    useEffect(() => {
        setSubscriptionToEdit(subscription || { settings: {} });
    }, [subscription]);

    const doSetEmail = useCallback((send: boolean | undefined) => {
        setChannel(subscriptionToEdit, 'email', send);
    }, [setSubscriptionToEdit]);

    const doSetPush = useCallback((send: boolean | undefined) => {
        setChannel(subscriptionToEdit, 'webpush', send);
    }, [setSubscriptionToEdit]);

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
            <div>
                <div class='notifo-form-group'>
                    <Toggle indeterminate value={subscriptionToEdit.settings?.email?.send}
                        onChange={doSetEmail} />

                    <label class='notifo-toggle-label'>{config.texts.notifyBeEmail}</label>
                </div>

                <div class='notifo-form-group'>
                    <Toggle indeterminate value={subscriptionToEdit.settings?.webpush?.send}
                        onChange={doSetPush} />

                    <label class='notifo-toggle-label'>{config.texts.notifyBeWebPush}</label>
                </div>

                <div class='notifo-form-group'>
                    <button class='notifo-action-button'>
                        {config.texts.unsubscribe}
                    </button>

                    <button class='notifo-action-button notifo-action-button-primary'>
                        {config.texts.save}
                    </button>
                </div>
            </div>
        </Modal>
    );
};

function setChannel(subscription: Subscription, channel: string, send?: boolean) {
    if (!subscription.settings) {
        subscription.settings = {};
    }

    if (!subscription.settings[channel]) {
        subscription.settings[channel] = { send };
    } else {
        subscription.settings[channel].send = send;
    }
}
