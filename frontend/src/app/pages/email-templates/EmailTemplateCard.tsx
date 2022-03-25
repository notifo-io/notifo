/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { match, NavLink } from 'react-router-dom';
import { Badge, Card, CardBody, DropdownItem, DropdownMenu, DropdownToggle, UncontrolledDropdown } from 'reactstrap';
import { Confirm, FormatDate, Icon, IFrame } from '@app/framework';
import { ChannelTemplateDto, Clients } from '@app/service';
import { texts } from '@app/texts';
import { combineUrl } from '@sdk/shared';

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
            const response = await Clients.EmailTemplates.getPreview(appId, template.id);

            setPreview(await response.data.text());
        }

        loadPreview();
    }, [appId, template.id]);

    const doDelete = React.useCallback(() => {
        onDelete(template);
    }, [onDelete, template]);

    const url = combineUrl(match.url, template.id);

    return (
        <NavLink className='card-link' to={url}>
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
                        <Badge color='primary' pill>{texts.common.primary}</Badge>
                    }

                    <Badge color='secondary' pill>{template.kind || 'Interpolation'}</Badge>

                    <div className='updated'>
                        <small><FormatDate date={template.lastUpdate} /></small>
                    </div>

                    <Confirm onConfirm={doDelete} text={texts.emailTemplates.confirmDelete}>
                        {({ onClick }) => (
                            <UncontrolledDropdown>
                                <DropdownToggle size='sm' nav>
                                    <Icon type='more' />
                                </DropdownToggle>
                                <DropdownMenu right>
                                    <DropdownItem onClick={onClick}>
                                        {texts.common.delete}
                                    </DropdownItem>
                                </DropdownMenu>
                            </UncontrolledDropdown>
                        )}
                    </Confirm>
                </CardBody>
            </Card>
        </NavLink>
    );
};
