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
import { TopicDto } from '@app/service';
import { CounterRow } from '@app/shared/components';
import { texts } from '@app/texts';

export interface TopicRowProps {
    // The topic.
    topic: TopicDto;
    
    // The language.
    language: string;

    // True to show all counters.
    showCounters?: boolean;

    // The edit event.
    onEdit?: (topic: TopicDto) => void;

    // The delete event.
    onDelete?: (topic: TopicDto) => void;
}

export const TopicRow = React.memo((props: TopicRowProps) => {
    const {
        language, 
        showCounters, 
        onDelete,
        onEdit,
        topic,
    } = props;

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    const doDelete = () => {
        onDelete && onDelete(topic);
    };

    const doEdit = () => {
        onEdit && onEdit(topic);
    };

    return (
        <CounterRow counters={topic.counters} columnCount={5} showCounters={showCounters}>
            <tr className='list-item-summary'>
                <td>
                    <span className='truncate'>{topic.path}</span>
                </td>
                <td>
                    <span className='truncate'>{topic.name?.[language]}</span>
                </td>
                <td>
                    {topic.showAutomatically ? texts.common.yes : ''}
                </td>
                <td>
                    {topic.isExplicit ? texts.common.yes : ''}
                </td>
                <td className='text-right'>
                    {topic.isExplicit &&
                        <>        
                            <Button className='ml-1' size='sm' color='primary' onClick={doEdit} data-tip={texts.common.edit}>
                                <Icon type='create' />
                            </Button>
        
                            <Confirm onConfirm={doDelete} text={texts.users.confirmDelete}>
                                {({ onClick }) => (
                                    <Button className='ml-1' size='sm' color='danger' onClick={onClick} data-tip={texts.common.delete}>
                                        <Icon type='delete' />
                                    </Button>
                                )}
                            </Confirm>
                        </>
                    }
                </td>
            </tr>
        </CounterRow>
    );
});
