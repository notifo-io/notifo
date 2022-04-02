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
    const archive = useStore(x => x.archive);
    const loaded = useStore(x => x.archiveLoaded);
    const loading = useStore(x => x.archiveLoading);

    useEffect(() => {
        dispatch(loadArchive(config));
    }, [dispatch, config]);

    return (
        <Fragment>
            {loaded && loading !== 'Failed' && archive.length === 0 &&
                <div class='notifo-list-empty'>{config.texts.notificationsEmpty}</div>
            }

            {loading === 'Failed' &&
                <div class='notifo-error'>{config.texts.loadingFailed}</div>
            }

            <div class='notifo-list-loading'>
                <Loader size={18} visible={loading === 'InProgress'} />
            </div>

            {archive.length > 0 &&
                <Fragment>
                    {archive.map(x => (
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
