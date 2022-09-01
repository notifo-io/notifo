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
import { SDKConfig, setSubscriptionChannel, setTopic, SubscriptionsDto } from '@sdk/shared';
import { loadSubscriptions, loadTopics, subscribe, useDispatch, useStore } from '@sdk/ui/model';
import { Modal } from './Modal';
import { TopicItem } from './TopicItem';
import { useMutable } from './utils';

export interface WebPushUIProps {
    // The main config.
    config: SDKConfig;

    // Triggered when confirmed.
    onAllow: () => void;

    // Triggered when denied.
    onDeny: () => void;
}

export const WebPushUI = (props: WebPushUIProps) => {
    const {
        config,
        onAllow,
        onDeny,
    } = props;

    const dispatch = useDispatch();
    const loaded = useStore(x => x.topicsLoaded);
    const formState = useMutable<SubscriptionsDto>({});
    const formValue = formState.current;
    const subscriptions = useStore(x => x.subscriptions);
    const topics = useStore(x => x.topics.filter(x => x.showAutomatically));

    useEffect(() => {
        dispatch(loadTopics(config));
    }, [dispatch, config]);

    useEffect(() => {
        dispatch(loadSubscriptions(config, topics.map(x => x.path)));
    }, [dispatch, config, topics]);

    useEffect(() => {
        formState.set(value => {
            for (const topic of topics) {
                value[topic.path] = subscriptions[topic.path]?.subscription || null;
            }
        });
    }, [formState, subscriptions, topics]);

    const doChangeTopic = useCallback((send: boolean | undefined, path: string) => {
        formState.set(value => setTopic(value, send, path));
    }, [formState]);

    const doChangeChannel = useCallback((send: boolean | undefined, path: string, channel: string) => {
        formState.set(value => setSubscriptionChannel(value[path]!, channel, send));
    }, [formState]);

    const doAllow = useCallback(() => {
        if (Object.keys(formValue).length > 0) {
            dispatch(subscribe(config, formValue));
        }

        onAllow();
    }, [config, dispatch, formValue, onAllow]);

    if (!loaded) {
        return null;
    }

    return (
        <div class='notifo'>
            <Modal position='top-global'>
                <h4>{config.texts.webpushConfirmTitle}</h4>

                <Fragment>
                    {topics.length > 0 &&
                        <Fragment>
                            <div class='notifo-topics-title'>
                                {config.texts.webpushTopics}
                            </div>

                            {topics.map(topic => 
                                <TopicItem key={topic.path}
                                    config={config}
                                    onChangeChannel={doChangeChannel}
                                    onChangeTopic={doChangeTopic}
                                    subscription={formValue[topic.path]}
                                    topic={topic} 
                                />,
                            )}
                        </Fragment>
                    }
                </Fragment>

                <div class='notifo-form-group'>
                    <small>{config.texts.webpushConfirmText}</small>
                </div>

                <div class='notifo-form-group'>
                    {config.permissionDeniedLifetimeHours > 0 ? (
                        <Fragment>
                            <button class='notifo-form-button primary' onClick={doAllow}>
                                {config.texts.allow}
                            </button>
        
                            <button class='notifo-form-button' onClick={onDeny}>
                                {config.texts.deny}
                            </button>
                        </Fragment>
                    ) : (
                        <Fragment>
                            <button class='notifo-form-button primary' onClick={doAllow}>
                                {config.texts.okay}
                            </button>
                        </Fragment>
                    )}
                </div>
            </Modal>
        </div>
    );
};
