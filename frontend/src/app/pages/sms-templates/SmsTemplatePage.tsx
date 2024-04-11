/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { useParams } from 'react-router';
import { NavLink } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Button, Card, CardBody, Col, Form, Row } from 'reactstrap';
import { FormError, Icon, Loader, Types, useEventCallback, useSimpleQuery } from '@app/framework';
import { Clients, TemplatePropertyDto } from '@app/service';
import { Forms, TemplateProperties } from '@app/shared/components';
import { loadSmsTemplate, updateSmsTemplate, useApp, useSmsTemplates } from '@app/state';
import { texts } from '@app/texts';
import 'codemirror/mode/django/django';

type FormValues = { name?: string; primary: boolean; languages: { [key: string]: string } };

const OPTIONS = {
    mode: 'django',
};

export const SmsTemplatePage = () => {
    const dispatch = useDispatch<any>();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const loadingTemplate = useSmsTemplates(x => x.loadingTemplate);
    const loadingTemplateError = useSmsTemplates(x => x.loadingTemplateError);
    const template = useSmsTemplates(x => x.template);
    const templateId = useParams().templateId!;
    const updating = useSmsTemplates(x => x.updating);
    const updatingError = useSmsTemplates(x => x.updatingError);
    const [language, setLanguage] = React.useState(appLanguages[0]);

    const properties = useSimpleQuery<TemplatePropertyDto[]>({
        queryKey: [appId],
        queryFn: async () => {
            const result = await Clients.SmsTemplates.getProperties(appId, abort);
            
            return result.items;
        },
        defaultValue: [],
    });

    React.useEffect(() => {
        dispatch(loadSmsTemplate({ appId, id: templateId }));
    }, [dispatch, appId, templateId]);

    React.useEffect(() => {
        if (loadingTemplateError) {
            toast.error(loadingTemplateError.response);
        }
    }, [loadingTemplateError]);

    React.useEffect(() => {
        if (updatingError) {
            toast.error(updatingError.response);
        }
    }, [updatingError]);

    const doSave = useEventCallback((values: FormValues) => {
        const update = { ...values, languages: {} as Record<string, any> };

        if (values?.languages) {
            for (const [key, value] of Object.entries(values.languages)) {
                update.languages[key] = { text: value };
            }
        }

        dispatch(updateSmsTemplate({ appId, id: templateId, update }));
    });

    const form = useForm<FormValues>({ mode: 'onChange' });

    React.useEffect(() => {
        const result: any = { ...Types.clone(template), languages: {} };

        if (template?.languages) {
            for (const [key, value] of Object.entries(template.languages)) {
                result.languages[key] = value.text;
            }
        }

        form.reset(result);
    }, [form, template]);

    return (
        <div className='sms-form'>
            <div className='header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <Row noGutters className='align-items-center'>
                            <Col xs='auto'>
                                <NavLink className='btn btn-back btn-flat' to='./../'>
                                    <Icon type='keyboard_arrow_left' />
                                </NavLink>
                            </Col>
                            <Col xs='auto'>
                                <h2>{texts.smsTemplates.singleHeader}</h2>
                            </Col>
                        </Row>
                    </Col>
                    <Col>
                        <Loader visible={loadingTemplate} />
                    </Col>
                </Row>
            </div>

            <Row>
                <Col xs={7}>
                    <FormProvider {...form}>
                        <Form onSubmit={form.handleSubmit(doSave)}>
                            <Card>
                                <CardBody>
                                    <fieldset disabled={updating}>
                                        <Forms.Text name='name' vertical
                                            label={texts.common.name} />

                                        <Forms.LocalizedCode name='languages' vertical
                                            languages={app.languages}
                                            language={language}
                                            onLanguageSelect={setLanguage}
                                            label={texts.common.templates} 
                                            initialOptions={OPTIONS} />

                                        <FormError error={updatingError} />

                                        <div className='d-flex justify-content-between mt-2 align-items-center'>
                                            <Forms.Boolean name='primary' vertical className='mb-0'
                                                label={texts.common.primary} />

                                            <div className='text-right'>
                                                <Button type='submit' color='success' disabled={updating}>
                                                    <Loader light small visible={updating} /> {texts.common.save}
                                                </Button>
                                            </div>
                                        </div>
                                    </fieldset>
                                </CardBody>
                            </Card>
                        </Form>
                    </FormProvider>
                </Col>
                <Col xs={5}>
                    <Card style={{ height: 'calc(100vh - 200px)' }}>
                        <CardBody className='overflow-auto'>
                            <TemplateProperties properties={properties.value} />
                        </CardBody>
                    </Card>
                </Col>
            </Row>
        </div>
    );
};
