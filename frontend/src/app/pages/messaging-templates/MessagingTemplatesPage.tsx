/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { toast } from 'react-toastify';
import { Button, Col, Label, Row } from 'reactstrap';
import { FormError, Icon, Loader, useEventCallback } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { createMessagingTemplate, deleteMessagingTemplate, loadMessagingTemplates, useApp, useMessagingTemplates } from '@app/state';
import { texts } from '@app/texts';
import { MessagingTemplateCard } from './MessagingTemplateCard';

export const MessagingTemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const creating = useMessagingTemplates(x => x.creating);
    const creatingError = useMessagingTemplates(x => x.creatingError);
    const deletingError = useMessagingTemplates(x => x.deletingError);
    const match = useRouteMatch();
    const messagingTemplates = useMessagingTemplates(x => x.templates);

    React.useEffect(() => {
        dispatch(loadMessagingTemplates(appId));
    }, [dispatch, appId]);

    React.useEffect(() => {
        if (creatingError) {
            toast.error(creatingError.response);
        }
    }, [creatingError]);

    React.useEffect(() => {
        if (deletingError) {
            toast.error(deletingError.response);
        }
    }, [deletingError]);

    const doCreate = useEventCallback(() => {
        dispatch(createMessagingTemplate({ appId }));
    });

    const doDelete = useEventCallback((template: ChannelTemplateDto) => {
        dispatch(deleteMessagingTemplate({ appId, id: template.id }));
    });

    return (
        <div className='messaging-templates'>
            <div className='align-items-center header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <h2 className='truncate'>{texts.messagingTemplates.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={messagingTemplates.isLoading} />
                    </Col>
                    <Col xs='auto'>
                        <Button color='success' onClick={doCreate}>
                            <Loader light small visible={creating} /> <Icon type='add' /> {texts.messagingTemplates.create}
                        </Button>
                    </Col>
                </Row>
            </div>

            <FormError error={messagingTemplates.error} />

            {messagingTemplates.items &&
                <>
                    {messagingTemplates.items.map(template => (
                        <MessagingTemplateCard key={template.id} template={template} match={match}
                            onDelete={doDelete}
                        />
                    ))}
                </>
            }

            {messagingTemplates.isLoaded && messagingTemplates.items?.length === 0 &&
                <div className='empty-button'>
                    <Label>{texts.messagingTemplates.notFound}</Label>

                    <Button size='lg' color='success' disabled={creating} onClick={doCreate}>
                        <Loader light small visible={creating} /> <Icon type='add' /> {texts.messagingTemplates.notFoundButton}
                    </Button>
                </div>
            }
        </div>
    );
};
