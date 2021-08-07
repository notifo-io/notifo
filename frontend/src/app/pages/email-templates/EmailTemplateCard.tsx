/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormatDate, Icon, IFrame } from '@app/framework';
import { Clients, ChannelTemplateDto } from '@app/service';
import { texts } from '@app/texts';
import { combineUrl } from '@sdk/shared';
import * as React from 'react';
import { match, NavLink } from 'react-router-dom';
import { Badge, Card, CardBody, DropdownItem, DropdownMenu, DropdownToggle, UncontrolledDropdown } from 'reactstrap';

export interface EmailTemplateCardProps {
    // The app id.
    appId: string;

    // The match.
    match: match;

    // The template.
    template: ChannelTemplateDto;

    // The delete event.
    onDelete: (template: ChannelTemplateDto) => void;
}

export const EmailTemplateCard = (props: EmailTemplateCardProps) => {
    const { appId, onDelete, match, template } = props;

    const [preview, setPreview] = React.useState<string | null>(null);

    React.useEffect(() => {
        async function loadPreview() {
            const response = await Clients.EmailTemplates.getPreviewImage(appId, template.id);

            setPreview(await response.data.text());
        }

        loadPreview();
    }, [appId, template.id]);

    const doDelete = React.useCallback(() => {
        onDelete(template);
    }, [template]);

    const url = combineUrl(match.url, template.id);

    return (
        <Card className='email-template'>
            <div className='email-template-preview'>
                <IFrame scrolling="no" html={preview} />
            </div>

            {template.primary &&
                <Badge color='primary' pill>{texts.emailTemplates.primary}</Badge>
            }

            <CardBody>
                <NavLink to={url}>
                    <h4>{template.name || texts.common.noName}</h4>
                </NavLink>

                <div className='mb-2'>
                    <small><FormatDate date={template.lastUpdate} /></small>
                </div>

                <NavLink to={url}>
                    {texts.common.edit}
                </NavLink>

                <UncontrolledDropdown>
                    <DropdownToggle size='sm' nav>
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
