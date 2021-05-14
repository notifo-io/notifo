/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { DropZone, FormError, Icon, ListSearch, Loader, Query } from '@app/framework';
import { MediaDto } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { MediaCard } from '@app/shared/components/MediaCard';
import { deleteMediaAsync, getApp, loadMediaAsync, uploadMediaAsync, useApps, useMedia } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Row } from 'reactstrap';

export const MediaPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const media = useMedia(x => x.media);

    React.useEffect(() => {
        dispatch(loadMediaAsync(appId));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadMediaAsync(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
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
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2>{texts.media.header}</h2>
                        </Col>
                        <Col xs='auto'>
                            {media.isLoading ? (
                                <Loader visible={media.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} md={7}>
                    <ListSearch list={media} onSearch={doLoad} placeholder={texts.media.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={media.error} />

            <div className='mb-4'>
                <DropZone files={{ onlyImages: true }} onDrop={doUpload} />

                {media.items &&
                    <>
                        {media.items.map(media => (
                            <MediaCard key={media.fileName} media={media}
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

            <TableFooter list={media} noDetailButton
                onChange={doLoad} />
        </div>
    );
};
