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
import { useDispatch } from 'react-redux';
import { Button, Form } from 'reactstrap';
import * as Yup from 'yup';

const FormSchema = Yup.object({
    // Required template code
    code: Yup.string()
        .label(texts.common.code).requiredI18n(),

    // Subject is required
    formatting: Yup.object({
        subject: Yup.object().label(texts.common.subject).atLeastOnStringI18n(),
    }),
});

export interface TemplateFormProps {
    // The template to edit.
    template?: TemplateDto;
}

export const TemplateForm = (props: TemplateFormProps) => {
    const { template } = props;

    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const upserting = useTemplates(x => x.upserting);
    const upsertingError = useTemplates(x => x.upsertingError);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);

    const doPublish = React.useCallback((params: TemplateDto) => {
        dispatch(upsertTemplateAsync({ appId, params }));
    }, []);

    const initialValues: any = template || {};

    return (
        <Formik<TemplateDto> initialValues={initialValues} onSubmit={doPublish} enableReinitialize validationSchema={FormSchema}>
            {({ handleSubmit }) => (
                <Form onSubmit={handleSubmit}>
                    <fieldset disabled={upserting}>
                        <Forms.Text name='code'
                            label={texts.common.code} />
                    </fieldset>

                    <NotificationsForm.Formatting
                        onLanguageSelect={setLanguage}
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
            )}
        </Formik>
    );
};
