/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Formik } from 'formik';
import * as React from 'react';
import { PushPreviewTarget } from 'react-push-preview';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, CardHeader, Col, DropdownItem, DropdownMenu, DropdownToggle, Form, Row, UncontrolledButtonDropdown } from 'reactstrap';
import * as Yup from 'yup';
import { FormError, Forms, Loader, Types } from '@app/framework';
import { TemplateDto } from '@app/service';
import { NotificationsForm } from '@app/shared/components';
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

const ALL_TARGETS: { target: PushPreviewTarget; label: string }[] = [{
    target: 'Notifo',
    label: 'Notifo',
}, {
    target: 'DesktopChrome',
    label: 'Chrome',
}, {
    target: 'DesktopMacOS2',
    label: 'Big Sur',
}, {
    target: 'DesktopMacOS',
    label: 'MacOS',
}, {
    target: 'MobileAndroid',
    label: 'Android',
}, {
    target: 'MobileIOS',
    label: 'iOS',
}];

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
    const [target, setTarget] = React.useState<PushPreviewTarget>('Notifo');

    const doPublish = React.useCallback((params: TemplateDto) => {
        dispatch(upsertTemplate({ appId, params }));
    }, [dispatch, appId]);

    const initialValues: any = React.useMemo(() => {
        const result: Partial<TemplateDto> = Types.clone(template || {});

        result.settings ||= {};

        for (const channel of CHANNELS) {
            result.settings[channel] ||= { send: 'Inherit' };
        }

        return result;
    }, [template]);

    return (
        <Formik<TemplateDto> initialValues={initialValues} enableReinitialize onSubmit={doPublish} validationSchema={FormSchema}>
            {({ handleSubmit, values }) => (
                <Card className='template-form slide-right'>
                    <CardHeader>
                        <Row className='align-items-center'>
                            <Col>
                                {template ? (
                                    <h3 className='truncate'>{texts.templates.templateEdit} {template.code}</h3>
                                ) : (
                                    <h3 className='truncate'>{texts.templates.templateNew}</h3>
                                )}
                            </Col>
                            <Col xs='auto'>
                                <button type="button" className="close" onClick={onClose}>
                                    <span aria-hidden="true">&times;</span>
                                </button>
                            </Col>
                        </Row>
                    </CardHeader>

                    <CardBody>
                        <Row className='template-form-inner'>
                            <Col>
                                <Form onSubmit={handleSubmit}>
                                    <fieldset disabled={upserting}>
                                        <Forms.Text name='code' vertical
                                            label={texts.common.code} />
                                    </fieldset>

                                    <NotificationsForm.Formatting vertical
                                        onLanguageSelect={onLanguageSelect}
                                        language={language}
                                        languages={appLanguages}
                                        field='formatting' disabled={upserting} />

                                    <NotificationsForm.Settings
                                        field='settings' disabled={upserting} />

                                    <FormError error={upsertingError} />

                                    <Button type='submit' color='success' disabled={upserting}>
                                        <Loader light small visible={upserting} /> {texts.common.save}
                                    </Button>
                                </Form>
                            </Col>
                            <Col xs='auto'>
                                <div className='template-form-preview sticky-top'>
                                    <NotificationPreview formatting={values?.formatting} language={language} target={target}></NotificationPreview>

                                    <UncontrolledButtonDropdown>
                                        <DropdownToggle color='primary' outline caret>
                                            {target}
                                        </DropdownToggle>
                                        <DropdownMenu right>
                                            {ALL_TARGETS.map(x =>
                                                <DropdownItem key={x.target} onClick={() => setTarget(x.target)}>
                                                    {x.label}
                                                </DropdownItem>,
                                            )}
                                        </DropdownMenu>
                                    </UncontrolledButtonDropdown>
                                </div>
                            </Col>
                        </Row>
                    </CardBody>
                </Card>
            )}
        </Formik>
    );
};
