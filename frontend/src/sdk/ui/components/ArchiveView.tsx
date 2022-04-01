/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Fragment, h } from 'preact';
import { useEffect } from 'preact/hooks';
import { NotificationsOptions, SDKConfig } from '@sdk/shared';
import { loadArchive, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { NotificationItem } from './NotificationItem';

export interface ArchiveViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;
}

export const ArchiveView = (props: ArchiveViewProps) => {
    const { config, options } = props;

    const dispatch = useDispatch();
    const isLoaded = useStore(x => x.archiveStatus !== 'InProgress');
    const isLoading = useStore(x => x.archiveStatus === 'InProgress');
    const notifications = useStore(x => x.archive);

    useEffect(() => {
        loadArchive(config, dispatch);
    }, [dispatch, config]);

    return (
        <Fragment>
            {isLoading && notifications.length === 0 &&
                <div class='notifo-list-loading'>
                    <Loader size={18} visible={true} />
                </div>
            }

            {isLoaded && notifications.length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
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
        </Fragment>
    );
};
