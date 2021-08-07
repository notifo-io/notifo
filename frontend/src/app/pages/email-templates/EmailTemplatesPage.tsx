/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { createEmailTemplate, deleteEmailTemplate, getApp, loadEmailTemplates, useApps, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, Col, Row } from 'reactstrap';
import { EmailTemplateCard } from './EmailTemplateCard';

export const EmailsPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const emailTemplates = useEmailTemplates(x => x.templates);
    const creating = useEmailTemplates(x => x.creating);
    const creatingError = useEmailTemplates(x => x.creatingError);
    const deletingError = useEmailTemplates(x => x.deletingError);

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
        <>
            <div className='email-header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <h2 className='truncate'>{texts.emailTemplates.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={emailTemplates.isLoading} />
                    </Col>
                    <Col xs='auto'>
                        <Button color='success'>
                            <Loader light small visible={creating} /> <Icon type='add' /> {texts.emailTemplates.create}
                        </Button>
                    </Col>
                </Row>
            </div>

            <FormError error={emailTemplates.error} />

            {emailTemplates.items &&
                <>
                    {emailTemplates.items.map(template => (
                        <EmailTemplateCard key={template.id} template={template}
                            onDelete={doDelete}
                        />
                    ))}
                </>
            }

            {!emailTemplates.isLoading && emailTemplates.items && emailTemplates.items.length === 0 &&
                <Button size='lg' color='success' disabled={creating} onClick={doCreate}>
                    <Loader light small visible={creating} /> <Icon type='add' /> {texts.emailTemplates.notFoundButton}
                </Button>
            }
        </>
    );
};
