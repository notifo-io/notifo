/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Modal, ModalBody, ModalFooter, ModalHeader, Row } from 'reactstrap';
import { FormError, Icon, ListSearch, Loader, Query } from '@app/framework';
import { MediaDto } from '@app/service';
import { loadMedia, useApp, useMedia } from '@app/state';
import { texts } from '@app/texts';
import { MediaCard } from './MediaCard';
import { TableFooter } from './TableFooter';

export interface MediaPickerProps {
    // The selected url.
    selectedUrl?: string;

    // Triggered when selected.
    onSelected?: (url: string) => void;

    // Triggered when just closed.
    onClose?: () => void;
}

export const MediaPicker = (props: MediaPickerProps) => {
    const {
        selectedUrl,
        onClose,
        onSelected,
    } = props;

    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const medias = useMedia(x => x.media);
    const [selection, setSelection] = React.useState<string>();

    React.useEffect(() => {
        dispatch(loadMedia(appId));
    }, [dispatch, appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadMedia(appId));
    }, [dispatch, appId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadMedia(appId, q));
    }, [dispatch, appId]);

    const doSelectMedia = React.useCallback((media: MediaDto) => {
        setSelection(media.url);
    }, []);

    const doSelect = React.useCallback(() => {
        selection && onSelected && onSelected(selection);
    }, [onSelected, selection]);

    const currentUrl = selection || selectedUrl;

    return (
        <Modal isOpen={true} size='lg' backdrop={false} toggle={onClose} className='media-picker'>
            <ModalHeader toggle={onClose}>
                <Row className='align-items-center'>
                    <Col xs={12} md={5}>
                        <Row className='align-items-center flex-nowrap'>
                            <Col>
                                {texts.media.header}
                            </Col>
                            <Col xs='auto'>
                                {medias.isLoading ? (
                                    <Loader visible={medias.isLoading} />
                                ) : (
                                    <Button color='blank' size='sm' className='btn-flat' onClick={doRefresh} data-tip={texts.common.refresh}>
                                        <Icon className='text-lg' type='refresh' />
                                    </Button>
                                )}
                            </Col>
                        </Row>
                    </Col>
                    <Col xs={12} md={7}>
                        <ListSearch list={medias} onSearch={doLoad} placeholder={texts.media.searchPlaceholder} />
                    </Col>
                </Row>
            </ModalHeader>

            <ModalBody>
                <FormError error={medias.error} />

                <div className='mb-4'>
                    {medias.items &&
                        <>
                            {medias.items.map(media => (
                                <MediaCard key={media.fileName} selected={media.url === currentUrl} media={media}
                                    onClick={doSelectMedia} />
                            ))}
                        </>
                    }

                    {!medias.isLoading && medias.items && medias.items.length === 0 &&
                        <div className='list-item-empty'>
                            {texts.media.mediaNotFound}
                        </div>
                    }
                </div>

                <TableFooter list={medias} noDetailButton
                    onChange={doLoad} />
            </ModalBody>

            <ModalFooter>
                <Button color='success' disabled={!selection || selection === selectedUrl} onClick={doSelect}>
                    {texts.common.select}
                </Button>
            </ModalFooter>
        </Modal>
    );
};
