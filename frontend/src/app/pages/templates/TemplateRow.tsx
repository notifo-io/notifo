/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button } from 'reactstrap';
import { Confirm, Icon, useEventCallback } from '@app/framework';
import { TemplateDto } from '@app/service';
import { texts } from '@app/texts';

export interface TemplateRowProps {
    // The template.
    template: TemplateDto;

    // True when selected.
    selected?: boolean;

    // The publish event.
    onPublish?: (user: TemplateDto) => void;

    // The edit event.
    onEdit?: (user: TemplateDto) => void;

    // The delete event.
    onDelete?: (user: TemplateDto) => void;
}

export const TemplateRow = React.memo((props: TemplateRowProps) => {
    const {
        onDelete,
        onEdit,
        onPublish,
        selected,
        template,
    } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = useEventCallback(() => {
        onDelete && onDelete(template);
    });

    const doEdit = useEventCallback(() => {
        onEdit && onEdit(template);
    });

    const doPublish = useEventCallback(() => {
        onPublish && onPublish(template);
    });

    return (
        <>
            <tr className={classNames('list-item-summary', { selected })}>
                <td onClick={doEdit}>
                    <span className='truncate mono'>{template.code}</span>
                </td>
                <td className='text-right'>
                    <Button className='ml-1' size='sm' color='info' onClick={doPublish} data-tip={texts.common.publish}>
                        <Icon type='send' />
                    </Button>

                    <Button className='ml-1' size='sm' color='primary' onClick={doEdit} data-tip={texts.common.edit}>
                        <Icon type='create' />
                    </Button>

                    <Confirm onConfirm={doDelete} text={texts.templates.confirmDelete}>
                        {({ onClick }) => (
                            <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                                <Icon type='delete' />
                            </Button>
                        )}
                    </Confirm>
                </td>
            </tr>
            <tr className='list-item-separator'></tr>
        </>
    );
});
