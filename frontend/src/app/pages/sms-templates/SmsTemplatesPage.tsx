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
import { FormError, Icon, Loader } from '@app/framework';
import { ChannelTemplateDto } from '@app/service';
import { createSmsTemplate, deleteSmsTemplate, loadSmsTemplates, useApp, useSmsTemplates } from '@app/state';
import { texts } from '@app/texts';
import { SmsTemplateCard } from './SmsTemplateCard';

export const SmsTemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const creating = useSmsTemplates(x => x.creating);
    const creatingError = useSmsTemplates(x => x.creatingError);
    const deletingError = useSmsTemplates(x => x.deletingError);
    const match = useRouteMatch();
    const smsTemplates = useSmsTemplates(x => x.templates);

    React.useEffect(() => {
        dispatch(loadSmsTemplates(appId));
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

    const doCreate = React.useCallback(() => {
        dispatch(createSmsTemplate({ appId }));
    }, [dispatch, appId]);

    const doDelete = React.useCallback((template: ChannelTemplateDto) => {
        dispatch(deleteSmsTemplate({ appId, id: template.id }));
    }, [dispatch, appId]);

    return (
        <div className='sms-templates'>
            <div className='align-items-center header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <h2 className='truncate'>{texts.smsTemplates.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={smsTemplates.isLoading} />
                    </Col>
                    <Col xs='auto'>
                        <Button color='success' onClick={doCreate}>
                            <Loader light small visible={creating} /> <Icon type='add' /> {texts.smsTemplates.create}
                        </Button>
                    </Col>
                </Row>
            </div>

            <FormError error={smsTemplates.error} />

            {smsTemplates.items &&
                <>
                    {smsTemplates.items.map(template => (
                        <SmsTemplateCard key={template.id} template={template} match={match}
                            onDelete={doDelete}
                        />
                    ))}
                </>
            }

            {!smsTemplates.isLoading && smsTemplates.items?.length === 0 &&
                <div className='empty-button'>
                    <Label>{texts.smsTemplates.notFound}</Label>

                    <Button size='lg' color='success' disabled={creating} onClick={doCreate}>
                        <Loader light small visible={creating} /> <Icon type='add' /> {texts.smsTemplates.notFoundButton}
                    </Button>
                </div>
            }
        </div>
    );
};
