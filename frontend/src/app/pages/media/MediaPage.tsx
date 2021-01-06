/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { DropZone, FormError, Icon, ListSearch, Loader, Query } from '@app/framework';
import { MediaDto } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { deleteMediaAsync, loadMediaAsync, uploadMediaAsync, useApps, useMedia } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Row } from 'reactstrap';
import { MediaCard } from './MediaCard';

export const MediaPage = () => {
    const dispatch = useDispatch();
    const appId = useApps(x => x.appId);
    const media = useMedia(x => x.media);

    React.useEffect(() => {
        dispatch(loadMediaAsync(appId));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadMediaAsync(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Query) => {
        dispatch(loadMediaAsync(appId, q));
    }, [appId]);

    const doDelete = React.useCallback((media: MediaDto) => {
        dispatch(deleteMediaAsync({ appId, fileName: media.fileName }));
    }, [appId]);

    const doUpload = React.useCallback((files: File[]) => {
        for (const file of files) {
            dispatch(uploadMediaAsync({ appId, file }));
        }
    }, [appId]);

    return (
        <div className='medias'>
            <Row className='align-items-center header'>
                <Col xs='auto'>
                    <h2>{texts.media.header}</h2>
                </Col>
                <Col>
                    {media.isLoading ? (
                        <Loader visible={media.isLoading} />
                    ) : (
                        <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                            <Icon className='text-lg' type='refresh' />
                        </Button>
                    )}
                </Col>
                <Col xs={3}>
                    <ListSearch list={media} onSearch={doLoad} placeholder={texts.media.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={media.error} />

            <div>
                <DropZone files={{ onlyImages: true }} onDrop={doUpload} />

                {media.items &&
                    <>
                        {media.items.map(media => (
                            <MediaCard key={media.fileName} media={media} appId={appId}
                                onDelete={doDelete}
                            />
                        ))}
                    </>
                }

                {!media.isLoading && media.items && media.items.length === 0 &&
                    <div className='list-item-empty'>
                        {texts.media.mediaNotFound}
                    </div>
                }
            </div>

            <TableFooter list={media} noCounters
                onChange={doLoad} />
        </div>
    );
};
