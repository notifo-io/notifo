/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Icon, Loader, Types } from '@app/framework';
import { loadSmsTemplate, updateSmsTemplate, useApp, useSmsTemplates } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { NavLink } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Button, Card, CardBody, Col, Form, Row } from 'reactstrap';

type FormValues = { name?: string; primary: boolean; languages: { [key: string]: string } };

export const SmsTemplatePage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const loadingTemplate = useSmsTemplates(x => x.loadingTemplate);
    const loadingTemplateError = useSmsTemplates(x => x.loadingTemplateError);
    const match = useRouteMatch();
    const template = useSmsTemplates(x => x.template);
    const templateId = match.params['templateId'];
    const updating = useSmsTemplates(x => x.updating);
    const updatingError = useSmsTemplates(x => x.updatingError);
    const [language, setLanguage] = React.useState(appLanguages[0]);

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

    const doSave = React.useCallback((values: FormValues) => {
        const update = { ...values, languages: {} };

        if (values?.languages) {
            for (const key of Object.keys(values.languages)) {
                update.languages[key] = { text: values.languages[key] };
            }
        }

        dispatch(updateSmsTemplate({ appId, id: templateId, update }));
    }, [dispatch, appId, templateId]);

    const initialValues = React.useMemo(() => {
        const result: any = { ...Types.clone(template), languages: {} };

        if (template?.languages) {
            for (const key of Object.keys(template.languages)) {
                result.languages[key] = template.languages[key].text;
            }
        }

        return result;
    }, [template]);

    return (
        <div className='sms-form'>
            <div className='header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <Row noGutters className='align-items-center'>
                            <Col xs='auto'>
                                <NavLink className='btn btn-back btn-flat' to='./'>
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

            <Formik<FormValues> initialValues={initialValues} enableReinitialize onSubmit={doSave}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <Card>
                            <CardBody>
                                <fieldset disabled={updating}>
                                    <Forms.Text name='name'
                                        label={texts.common.name} />

                                    <Forms.Boolean name='primary'
                                        label={texts.common.primary} />

                                    <Forms.LocalizedTextArea name='languages'
                                        languages={app.languages}
                                        language={language}
                                        onLanguageSelect={setLanguage}
                                        label={texts.common.templates} />
                                </fieldset>

                                <FormError error={updatingError} />

                                <div className='text-right mt-2'>
                                    <Button type='submit' color='success' disabled={updating}>
                                        <Loader light small visible={updating} /> {texts.common.save}
                                    </Button>
                                </div>
                            </CardBody>
                        </Card>
                    </Form>
                )}
            </Formik>
        </div>
    );
};
