/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useCallback, useEffect, useRef } from 'preact/hooks';
import { NotificationsOptions, SDKConfig, setSubscriptionChannel, setTopic, SubscriptionsDto } from '@sdk/shared';
import { getTopicsLoadingStatus, getTopicsUpdateStatus, loadSubscriptions, subscribe, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { TopicItem } from './TopicItem';
import { useMutable } from './utils';

export interface TopicsViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;
}

export const TopicsView = (props: TopicsViewProps) => {
    const { config } = props;

    const dispatch = useDispatch();
    const formState = useMutable<SubscriptionsDto>({});
    const formValue = formState.current;
    const loaded = useStore(x => x.topicsLoaded);
    const loading = useStore(x => getTopicsLoadingStatus(x));
    const subscriptions = useStore(x => x.subscriptions);
    const topics = useStore(x => x.topics);
    const topicsRef = useRef(topics);
    const updating = useStore(x => getTopicsUpdateStatus(x));

    useEffect(() => {
        dispatch(loadSubscriptions(config, topicsRef.current.map(x => x.path)));
    }, [dispatch, config]);

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

    const doSave = useCallback(() => {
        dispatch(subscribe(config, formValue));
    }, [config, dispatch, formValue]);

    const disabled = loading == 'InProgress' || updating === 'InProgress';

    return (
        <Fragment>
            {loaded && Object.keys(topics).length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
            }

            <div class='notifo-list-loading'>
                <Loader size={18} visible={loading === 'InProgress'} />
            </div>

            {loading === 'Failed' &&
                <div class='notifo-error'>{config.texts.loadingFailed}</div>
            }

            {topics.length > 0 &&
                <Fragment>
                    {topics.map(topic => 
                        <TopicItem key={topic}
                            config={config}
                            disabled={disabled}
                            onChangeChannel={doChangeChannel}
                            onChangeTopic={doChangeTopic}
                            subscription={formValue[topic.path]}
                            topic={topic} 
                        />,
                    )}

                    <div class='notifo-form-group'>
                        {updating === 'Failed' &&
                            <div class='notifo-error'>{config.texts.savingFailed}</div>
                        }

                        <button class='notifo-form-button primary' type='submit' onClick={doSave} disabled={disabled}>
                            {config.texts.save}
                        </button>

                        <Loader size={16} visible={updating === 'InProgress'} />
                    </div>
                </Fragment>
            }
        </Fragment>
    );
};