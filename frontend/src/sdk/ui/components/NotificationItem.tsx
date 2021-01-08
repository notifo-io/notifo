/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { NotificationsOptions, NotifoNotification, SDKConfig, SUPPORTED_LOCALES, withPreset } from '@sdk/shared';
import { formatDistanceToNow, parseISO } from 'date-fns';
import { useCallback, useEffect, useMemo, useState } from 'preact/hooks';
import { Image } from './Image';
import { Loader } from './Loader';
import { useInView } from './observer';

export type UpdateState = 'InProgress' | 'Failed' | 'None' | 'Done';

export interface NotificationItemProps {
    // The confirmation button.
    notification: NotifoNotification;

    // The modal element.
    modal: HTMLElement;

    // The options.
    options: NotificationsOptions;

    // The main config.
    config: SDKConfig;

    // Clicked when a notification is confirmed.
    onConfirm?: (notification: NotifoNotification) => Promise<any>;

    // Clicked when a notification is seen.
    onSeen?: (notification: NotifoNotification) => Promise<any>;
}

export const NotificationItem = (props: NotificationItemProps) => {
    const {
        config,
        modal,
        notification,
        onConfirm,
        onSeen,
    } = props;

    const [ref, setRef] = useState<HTMLElement>(null);
    const [markingSeen, setMarkingSeen] = useState<UpdateState>('None');
    const [markingConfirm, setMarkConfirm] = useState<UpdateState>('None');

    const inView = useInView(ref, modal);

    const doSee = useCallback(async () => {
        try {
            setMarkingSeen('InProgress');

            await onSeen(notification);

            setMarkingSeen('Done');
        } catch {
            setMarkingSeen('Failed');
        }
    }, [notification]);

    const doConfirm = useCallback(async () => {
        try {
            setMarkConfirm('InProgress');

            await onConfirm(notification);

            setMarkConfirm('Done');
        } catch {
            setMarkConfirm('Failed');
        }
    }, [notification, onConfirm]);

    useEffect(() => {
        if (inView && !notification.isSeen) {
            const timer = setTimeout(() => {
                doSee();
            }, 2000);

            return () => {
                clearTimeout(timer);
            };
        }

        return null;
    }, [inView, notification, doSee]);

    const time = useMemo(() => {
        if (!notification.created) {
            return null;
        }

        const locale = SUPPORTED_LOCALES[config.locale];

        return formatDistanceToNow(parseISO(notification.created), { locale });
    }, [config.locale, notification.created]);

    return (
        <div class='notifo-notification' ref={setRef}>
            {!notification.isSeen && markingSeen !== 'Failed' &&
                <span class='notifo-notification-new'></span>
            }

            <Image className='notifo-notification-image-large' src={withPreset(notification.imageLarge, 'WebLarge')} />

            <div class='notifo-notification-row2'>
                <Image className='notifo-notification-image-small notifo-notification-left' src={withPreset(notification.imageSmall, 'WebSmall')} />

                <div class='notifo-notification-right'>
                    {notification.subject &&
                        <div class='notifo-notification-subject'>
                            {notification.linkUrl && !notification.linkText ? (
                                <a href={notification.linkUrl} target='_blank' rel='noopener'>{notification.subject}</a>
                            ) : (
                                    <span>{notification.subject}</span>
                            )}
                        </div>
                    }

                    {notification.body &&
                        <div class='notifo-notification-body'>
                            {notification.body}
                        </div>
                    }

                    {notification.linkUrl && notification.linkText &&
                        <div class='notifo-notification-link'>
                            <a href={notification.linkUrl} target='_blank' rel='noopener'>{notification.linkText}</a>
                        </div>
                    }

                    {time &&
                        <div class='notifo-notification-time'>
                            {time}
                        </div>
                    }

                    {notification.confirmText && notification.confirmUrl && !notification.isConfirmed &&
                        <button class='notifo-notification-confirm' onClick={doConfirm}>
                            <Loader size={12} visible={markingConfirm === 'InProgress'} />

                            {notification.confirmText}
                        </button>
                    }
                </div>
            </div>
        </div>
    );
};
