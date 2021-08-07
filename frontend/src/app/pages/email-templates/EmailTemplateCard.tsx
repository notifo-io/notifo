/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import { Card, CardBody, CardHeader, DropdownItem, DropdownMenu, DropdownToggle, UncontrolledDropdown } from 'reactstrap';

export interface EmailTemplateCardProps {
    // The template.
    template: ChannelTemplateDto;

    // The delete event.
    onDelete: (template: ChannelTemplateDto) => void;
}

export const EmailTemplateCard = (props: EmailTemplateCardProps) => {
    const { onDelete, template } = props;

    const doDelete = React.useCallback(() => {
        onDelete(template);
    }, [template]);

    return (
        <Card className='email-template'>
            <CardHeader>

            </CardHeader>

            <CardBody>
                <h3>{template.name}</h3>

                <UncontrolledDropdown>
                    <DropdownToggle nav caret>
                        <Icon type='more' />
                    </DropdownToggle>
                    <DropdownMenu right>
                        <DropdownItem onClick={doDelete}>
                            {texts.common.delete}
                        </DropdownItem>
                    </DropdownMenu>
                </UncontrolledDropdown>
            </CardBody>
        </Card>
    );
};
