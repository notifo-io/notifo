/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { Fragment, h } from 'preact';

import { NotificationsOptions, SDKConfig } from '@sdk/shared';
import { loadArchive, useDispatch, useStore } from '@sdk/ui/model';
import { useCallback, useEffect } from 'preact/hooks';
import { Icon } from './Icon';
import { Loader } from './Loader';
import { NotificationItem } from './NotificationItem';

export interface NotificationsArchiveProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // To close this view.
    onClose?: () => void;
}

export const NotificationsArchive = (props: NotificationsArchiveProps) => {
    const {
        config,
        onClose,
        options,
    } = props;

    const dispatch = useDispatch();
    const notifications = useStore(x => x.archive);
    const isLoading = useStore(x => x.archiveTransition === 'InProgress');
    const isLoaded = useStore(x => x.archiveTransition !== 'InProgress');

    useEffect(() => {
        loadArchive(config, dispatch);
    }, []);

    const doClose = useCallback((event: Event) => {
        onClose && onClose();

        event.preventDefault();
        event.stopImmediatePropagation();
        event.stopPropagation();
    }, [onClose]);

    return (
        <div class='notifo-notifications-archive'>
            <div class='notifo-header'>
                <button type='button' onClick={doClose}>
                    <Icon type='back' size={20} />
                </button>

                {config.texts.archive}
            </div>

            {isLoading && notifications.length === 0 &&
                <div class='notifo-loading'>
                    <Loader size={18} visible={true} />
                </div>
            }

            {isLoaded && notifications.length === 0 &&
                <div class='notifo-empty'>{config.texts.notificationsEmpty}</div>
            }

            {isLoaded && notifications.length > 0 &&
                <Fragment>
                    {notifications.map(x => (
                        <NotificationItem key={x.id}
                            config={config}
                            notification={x}
                            options={options}
                            disabled
                        />
                    ))}
                </Fragment>
            }
        </div>
    );
};
