/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader } from '@app/framework';
import { TemplateDto } from '@app/service';
import { NotificationsForm } from '@app/shared/components';
import { getApp, upsertTemplateAsync, useApps, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { PushPreviewTarget } from 'react-push-preview';
import { useDispatch } from 'react-redux';
import { Button, ButtonGroup, Form } from 'reactstrap';
import * as Yup from 'yup';
import { NotificationPreview } from './NotificationPreview';

const FormSchema = Yup.object({
    // Required template code
    code: Yup.string()
        .label(texts.common.code).requiredI18n(),

    // Subject is required
    formatting: Yup.object({
        subject: Yup.object().label(texts.common.subject).atLeastOnStringI18n(),
    }),
});

const ALL_TARGETS: { target: PushPreviewTarget, label: string }[] = [{
    target: 'Notifo',
    label: 'Notifo',
}, {
    target: 'DeskopFirefox',
    label: 'Firefox',
}, {
    target: 'DesktopChrome',
    label: 'Chrome',
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
}

export const TemplateForm = (props: TemplateFormProps) => {
    const {
        language,
        onLanguageSelect,
        template,
    } = props;

    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const upserting = useTemplates(x => x.upserting);
    const upsertingError = useTemplates(x => x.upsertingError);
    const [target, setTarget] = React.useState<PushPreviewTarget>('Notifo');

    const doPublish = React.useCallback((params: TemplateDto) => {
        dispatch(upsertTemplateAsync({ appId, params }));
    }, []);

    const initialValues: any = template || {};

    return (
        <Formik<TemplateDto> initialValues={initialValues} onSubmit={doPublish} enableReinitialize validationSchema={FormSchema}>
            {({ handleSubmit, values }) => (
                <>
                    <div className='templates-column templates-form'>
                        <div className='templates-header'>
                            {template ? (
                                <h2 className='truncate'>{texts.templates.templateEdit} {template.code}</h2>
                            ) : (
                                <h2 className='truncate'>{texts.templates.templateNew}</h2>
                            )}
                        </div>

                        <div className='templates-body'>
                            <Form onSubmit={handleSubmit}>
                                <fieldset disabled={upserting}>
                                    <Forms.Text name='code'
                                        label={texts.common.code} />
                                </fieldset>

                                <NotificationsForm.Formatting
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
                        </div>
                    </div>

                    <div className='templates-column templates-preview'>
                        <div className='templates-header'>
                            <ButtonGroup size='sm'>
                                {ALL_TARGETS.map(x =>
                                    <Button key={x.target} type='button' color='primary' outline active={x.target === target}
                                        onClick={() => setTarget(x.target)}>
                                        {x.label}
                                    </Button>,
                                )}
                            </ButtonGroup>
                        </div>
                        <div className='templates-body'>
                            <NotificationPreview formatting={values?.formatting} language={language} target={target}></NotificationPreview>
                        </div>
                    </div>
                </>
            )}
        </Formik>
    );
};
