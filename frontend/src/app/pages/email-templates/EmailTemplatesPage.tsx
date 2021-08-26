/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { createEmailTemplate, deleteEmailTemplate, loadEmailTemplates, useApp, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { toast } from 'react-toastify';
import { Button, Col, Label, Row } from 'reactstrap';
import { EmailTemplateCard } from './EmailTemplateCard';

export const EmailTemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const creating = useEmailTemplates(x => x.creating);
    const creatingError = useEmailTemplates(x => x.creatingError);
    const deletingError = useEmailTemplates(x => x.deletingError);
    const emailTemplates = useEmailTemplates(x => x.templates);
    const match = useRouteMatch();

    React.useEffect(() => {
        dispatch(loadEmailTemplates(appId));
    }, [appId]);

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

    const doCreate = React.useCallback(() => {
        dispatch(createEmailTemplate({ appId }));
    }, [appId]);

    const doDelete = React.useCallback((template: ChannelTemplateDto) => {
        dispatch(deleteEmailTemplate({ appId, id: template.id }));
    }, [appId]);

    return (
        <div className='email-templates'>
            <div className='align-items-center header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <h2 className='truncate'>{texts.emailTemplates.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={emailTemplates.isLoading} />
                    </Col>
                    <Col xs='auto'>
                        <Button color='success' onClick={doCreate}>
                            <Loader light small visible={creating} /> <Icon type='add' /> {texts.emailTemplates.create}
                        </Button>
                    </Col>
                </Row>
            </div>

            <FormError error={emailTemplates.error} />

            {emailTemplates.items &&
                <>
                    {emailTemplates.items.map(template => (
                        <EmailTemplateCard key={template.id} template={template} appId={appId} match={match}
                            onDelete={doDelete}
                        />
                    ))}
                </>
            }

            {!emailTemplates.isLoading && emailTemplates.items?.length === 0 &&
                <div className='empty-button'>
                    <Label>{texts.emailTemplates.notFound}</Label>

                    <Button size='lg' color='success' disabled={creating} onClick={doCreate}>
                        <Loader light small visible={creating} /> <Icon type='add' /> {texts.emailTemplates.notFoundButton}
                    </Button>
                </div>
            }
        </div>
    );
};
