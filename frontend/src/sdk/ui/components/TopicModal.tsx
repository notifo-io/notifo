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
import {  SDKConfig, sendToBoolean, setSubscriptionChannel, SubscriptionDto } from '@sdk/shared';
import { Status, subscribe, useDispatch } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Toggle } from './Toggle';
import { useMutable } from './utils';

export interface TopicModalProps {
    // The main config.
    config: SDKConfig;

    // The subscription.
    subscription: SubscriptionDto | null;

    // The subscription.
    subscriptionStatus?: Status;

    // The topic to watch.
    topic: string;
}

export const TopicModal = (props: TopicModalProps) => {
    const {
        config,
        subscription,
        subscriptionStatus,
        topic,
    } = props;

    const dispatch = useDispatch();
    const formState = useMutable<SubscriptionDto>({ topicSettings: {} });
    const formValue = formState.current;
    const inProgress = subscriptionStatus === 'InProgress';

    useEffect(() => {
        formState.set(subscription || {} as any);
    }, [subscription, formState]);

    const doChangeChannel = useCallback((send: boolean | undefined, name: string) => {
        formState.set(v => setSubscriptionChannel(v, name, send));
    }, [formState]);

    const doSubscribe = useCallback(() => {
        dispatch(subscribe(config, { [topic]: formValue }));
    }, [dispatch, config, formValue, topic]);

    const doUnsubscribe = useCallback(() => {
        dispatch(subscribe(config, { [topic]: null }));
    }, [dispatch, config, topic]);

    return (
        <Fragment>
            {config.allowedChannels['email'] &&
                <div class='notifo-form-group'>
                    <Toggle indeterminate value={sendToBoolean(formValue.topicSettings?.email?.send)} name='email' disabled={inProgress}
                        onChange={doChangeChannel} />

                    <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                </div>
            }

            <div class='notifo-form-group'>
                <Toggle indeterminate value={sendToBoolean(formValue.topicSettings?.webpush?.send)} name='webpush' disabled={inProgress}
                    onChange={doChangeChannel} />

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

                <Loader size={16} visible={subscriptionStatus === 'InProgress'} />
            </div>
        </Fragment>
    );
};
