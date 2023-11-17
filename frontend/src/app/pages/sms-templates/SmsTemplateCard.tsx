/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { NavLink } from 'react-router-dom';
import { Badge, Card, CardBody, DropdownItem, DropdownMenu, DropdownToggle } from 'reactstrap';
import { Confirm, FormatDate, Icon, OverlayDropdown, useEventCallback } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { texts } from '@app/texts';

export interface SmsTemplateCardProps {
    // The template.
    template: ChannelTemplateDto;

    // The delete event.
    onDelete: (template: ChannelTemplateDto) => void;
}

export const SmsTemplateCard = (props: SmsTemplateCardProps) => {
    const { onDelete, template } = props;

    const doDelete = useEventCallback(() => {
        onDelete(template);
    });

    return (
        <NavLink className='card-link' to={template.id}>
            <Card className='sms-template'>
                <CardBody>
                    {template.name ? (
                        <h4 className='truncate'>{template.name}</h4>
                    ) : (
                        <h4 className='truncate text-muted'>{texts.common.noName}</h4>
                    )}

                    {template.primary &&
                        <Badge color='primary' pill>{texts.common.primary}</Badge>
                    }

                    <div className='updated'>
                        <small><FormatDate date={template.lastUpdate} /></small>
                    </div>

                    <Confirm onConfirm={doDelete} text={texts.emailTemplates.confirmDelete}>
                        {({ onClick }) => (
                            <OverlayDropdown button={
                                <DropdownToggle size='sm' nav>
                                    <Icon type='more' />
                                </DropdownToggle>
                            }>
                                <DropdownMenu>
                                    <DropdownItem onClick={onClick}>
                                        {texts.common.delete}
                                    </DropdownItem>
                                </DropdownMenu>
                            </OverlayDropdown>
                        )}
                    </Confirm>
                </CardBody>
            </Card>
        </NavLink>
    );
};
