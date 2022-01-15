/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, CardFooter } from 'reactstrap';
import { Confirm, Icon } from '@app/framework';
import { MediaDto } from '@app/service';
import { texts } from '@app/texts';

export interface MediaCardProps {
    // The media.
    media: MediaDto;

    // True if selected.
    selected?: boolean;

    // When clicked.
    onClick?: (media: MediaDto) => void;

    // The delete event.
    onDelete?: (media: MediaDto) => void;
}

export const MediaCard = React.memo((props: MediaCardProps) => {
    const {
        media,
        onClick,
        onDelete,
        selected,
    } = props;

    const [visible, setVisible] = React.useState(false);

    React.useEffect(() => {
        setVisible(true);
    }, [media.url]);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doHide = React.useCallback(() => {
        setVisible(false);
    }, []);

    const doDelete = React.useCallback(() => {
        onDelete && onDelete(media);
    }, [media, onDelete]);

    const doClick = React.useCallback(() => {
        onClick && onClick(media);
    }, [media, onClick]);

    const image = `${media.url}?width=200&height=150&mode=Pad`;

    return (
        <Card className='media-card' onClick={doClick} color={selected ? 'primary' : undefined}>
            <CardBody>
                <img className={classNames({ hidden: !visible })} src={image} onError={doHide} />
            </CardBody>
            <CardFooter>
                <div className='truncate'>{media.fileName}</div>

                <small>{media.fileInfo}</small>

                {onDelete &&
                    <Confirm onConfirm={doDelete} text={texts.media.confirmDelete}>
                        {({ onClick }) => (
                            <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                                <Icon type='delete' />
                            </Button>
                        )}
                    </Confirm>
                }
            </CardFooter>
        </Card>
    );
});
