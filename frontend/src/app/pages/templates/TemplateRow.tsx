/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Confirm, Icon } from '@app/framework';
import { TemplateDto } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import ReactTooltip from 'react-tooltip';
import { Button } from 'reactstrap';

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

export const TemplateRow = (props: TemplateRowProps) => {
    const { onDelete, onEdit, onPublish, template } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = React.useCallback(() => {
        onDelete && onDelete(template);
    }, [template]);

    const doEdit = React.useCallback(() => {
        onEdit && onEdit(template);
    }, [template]);

    const doPublish = React.useCallback(() => {
        onPublish && onPublish(template);
    }, [template]);

    return (
        <tr>
            <td>
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
    );
};
