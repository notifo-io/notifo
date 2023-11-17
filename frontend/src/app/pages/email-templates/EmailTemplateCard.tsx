/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { NavLink } from 'react-router-dom';
import { Badge, Card, CardBody, DropdownItem, DropdownMenu, DropdownToggle } from 'reactstrap';
import { Confirm, FormatDate, Icon, IFrame, OverlayDropdown, useEventCallback } from '@app/framework';
import { ChannelTemplateDto, Clients } from '@app/service';
import { texts } from '@app/texts';

export interface EmailTemplateCardProps {
    // The app id.
    appId: string;

    // The template.
    template: ChannelTemplateDto;

    // The delete event.
    onDelete: (template: ChannelTemplateDto) => void;
}

export const EmailTemplateCard = (props: EmailTemplateCardProps) => {
    const { appId, onDelete, template } = props;

    const [preview, setPreview] = React.useState<string | null>(null);

    React.useEffect(() => {
        async function loadPreview() {
            const response = await Clients.EmailTemplates.getPreview(appId, template.id);

            setPreview(await response.data.text());
        }

        loadPreview();
    }, [appId, template.id]);

    const doDelete = useEventCallback(() => {
        onDelete(template);
    });

    return (
        <NavLink className='card-link' to={template.id}>
            <Card className='email-template'>
                <div className='email-template-preview'>
                    <IFrame scrolling="no" html={preview} />
                </div>

                <CardBody>
                    {template.name ? (
                        <h4 className='truncate'>{template.name}</h4>
                    ) : (
                        <h4 className='truncate text-muted'>{texts.common.noName}</h4>
                    )}

                    {template.primary &&
                        <Badge color='primary' className='mr-1' pill>{texts.common.primary}</Badge>
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
