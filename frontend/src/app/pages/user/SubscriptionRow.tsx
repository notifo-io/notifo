/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button } from 'reactstrap';
import { Confirm, Icon } from '@app/framework';
import { SubscriptionDto } from '@app/service';
import { texts } from '@app/texts';

export interface SubscriptionRowProps {
    // The subscription.
    subscription: SubscriptionDto;

    // The publish event.
    onPublish?: (subscription: SubscriptionDto) => void;

    // The edit event.
    onEdit?: (subscription: SubscriptionDto) => void;

    // The delete event.
    onDelete?: (subscription: SubscriptionDto) => void;
}

export const SubscriptionRow = (props: SubscriptionRowProps) => {
    const { onDelete, onEdit, onPublish, subscription } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = React.useCallback(() => {
        onDelete && onDelete(subscription);
    }, [onDelete, subscription]);

    const doEdit = React.useCallback(() => {
        onEdit && onEdit(subscription);
    }, [onEdit, subscription]);

    const doPublish = React.useCallback(() => {
        onPublish && onPublish(subscription);
    }, [onPublish, subscription]);

    return (
        <tr>
            <td>
                <span className='truncate'>{subscription.topicPrefix}</span>
            </td>
            <td className='text-right'>
                <Button className='ml-1' size='sm' color='info' onClick={doPublish} data-tip={texts.common.publish}>
                    <Icon type='send' />
                </Button>

                <Button className='ml-1' size='sm' color='primary' onClick={doEdit} data-tip={texts.common.edit}>
                    <Icon type='create' />
                </Button>

                <Confirm onConfirm={doDelete} text={texts.subscriptions.confirmDelete}>
                    {({ onClick }) => (
                        <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                            <Icon type='delete' />
                        </Button>
                    )}
                </Confirm>
            </td>
        </tr>
    );
};
