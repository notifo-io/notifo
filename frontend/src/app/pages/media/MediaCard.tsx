/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Confirm, Icon } from '@app/framework';
import { getApiUrl, MediaDto } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, CardFooter } from 'reactstrap';

export interface MediaCardProps {
    // The media.
    media: MediaDto;

    // The app id.
    appId: string;

    // The delete event.
    onDelete?: (user: MediaDto) => void;
}

export const MediaCard = (props: MediaCardProps) => {
    const { appId, media, onDelete } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = React.useCallback(() => {
        onDelete && onDelete(media);
    }, [media]);

    const image =  `${getApiUrl()}/api/assets/${appId}/${media.fileName}?width=200&height=150&mode=Pad`;

    return (
        <Card className='media-card'>
            <CardBody>
                <img src={image} />
            </CardBody>
            <CardFooter>
                <div className='truncate'>{media.fileName}</div>

                <small>{media.fileInfo}</small>

                <Confirm onConfirm={doDelete} text={texts.media.confirmDelete}>
                    {({ onClick }) => (
                        <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                            <Icon type='delete' />
                        </Button>
                    )}
                </Confirm>
            </CardFooter>
        </Card>
    );
};
