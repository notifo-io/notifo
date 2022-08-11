/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, CardHeader, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';
import { FormError, Icon, Loader, Types } from '@app/framework';
import { TemplateDto } from '@app/service';
import { Forms, NotificationsForm } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { upsertTemplate, useApp, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import { NotificationPreview } from './NotificationPreview';

const FormSchema = Yup.object({
    // Required template code
    code: Yup.string()
        .label(texts.common.code).requiredI18n(),

    // Subject is required
    formatting: Yup.object({
        subject: Yup.object().label(texts.common.subject).atLeastOneStringI18n(),
    }),
});

export interface TemplateFormProps {
    // The template to edit.
    template?: TemplateDto;

    // The current language.
    language: string;

    // Triggered when the language is selected.
    onLanguageSelect: (language: string) => void;

    // Triggered when closed.
    onClose: () => void;
}

export const TemplateForm = (props: TemplateFormProps) => {
    const {
        language,
        onClose,
        onLanguageSelect,
        template,
    } = props;

    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const upserting = useTemplates(x => x.upserting);
    const upsertingError = useTemplates(x => x.upsertingError);
    const [viewFullscreen, setViewFullscreen] = React.useState<boolean>(false);

    const doPublish = React.useCallback((params: TemplateDto) => {
        dispatch(upsertTemplate({ appId, params }));
    }, [dispatch, appId]);

    const doToggleFullscreen = React.useCallback(() => {
        setViewFullscreen(x => !x);
    }, []);

    const initialValues: any = React.useMemo(() => {
        const result: Partial<TemplateDto> = Types.clone(template || {});

        result.settings ||= {};

        for (const channel of CHANNELS) {
            result.settings[channel] ||= { send: 'Inherit', condition: 'Inherit' };
        }

        return result;
    }, [template]);

    return (
        <Formik<TemplateDto> initialValues={initialValues} enableReinitialize onSubmit={doPublish} validationSchema={FormSchema}>
            {({ handleSubmit, values }) => (
                <Card className={classNames('template-form', 'slide-right', { ['fullscreen-mode']: viewFullscreen })}>
                    <CardHeader>
                        <Row className='align-items-center d-nowrap'>
                            <Col>
                                {template ? (
                                    <h3 className='truncate'>{texts.templates.templateEdit} {template.code}</h3>
                                ) : (
                                    <h3 className='truncate'>{texts.templates.templateNew}</h3>
                                )}
                            </Col>
                            <Col xs='auto'>
                                <Button type='submit' color='success' disabled={upserting}>
                                    <Loader light small visible={upserting} /> {texts.common.save}
                                </Button>
                            </Col>
                        </Row>

                        <button type='button' className='fullscreen' onClick={doToggleFullscreen}>
                            <Icon type={viewFullscreen ? 'fullscreen_exit' : 'fullscreen'} />
                        </button>

                        <button type='button' className='close' onClick={onClose}>
                            <span aria-hidden='true'>×</span>
                        </button>
                    </CardHeader>

                    <CardBody>
                        <Row className='template-form-inner'>
                            <Col xs='auto'>
                                <Form onSubmit={handleSubmit}>
                                    <FormError error={upsertingError} />

                                    <fieldset disabled={upserting}>
                                        <Forms.Text name='code' vertical
                                            label={texts.common.code} />
                                    </fieldset>

                                    <NotificationsForm.Formatting vertical
                                        onLanguageSelect={onLanguageSelect}
                                        language={language}
                                        languages={appLanguages}
                                        field='formatting' disabled={upserting} />

                                    <hr />

                                    <NotificationsForm.Settings
                                        field='settings' disabled={upserting} />
                                </Form>
                            </Col>
                            <Col xs='auto'>
                                <div className='template-form-preview sticky-top'>
                                    <Label>{texts.common.preview}</Label>
                                    
                                    <NotificationPreview formatting={values?.formatting} language={language}></NotificationPreview>
                                </div>
                            </Col>
                        </Row>
                    </CardBody>
                </Card>
            )}
        </Formik>
    );
};
