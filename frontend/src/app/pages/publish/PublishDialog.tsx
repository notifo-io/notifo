/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader, usePrevious } from '@app/framework';
import { PublishDto } from '@app/service';
import { NotificationsForm, TemplateInput } from '@app/shared/components';
import { getApp, publishAsync, togglePublishDialog, useApps, usePublish } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as Yup from 'yup';

const FormSchema = Yup.object({
    // Required topic name
    topic: Yup.string()
        .label(texts.common.name).requiredI18n().topicI18n(),

    // The mode (template or formatted).
    templated: Yup.boolean()
        .label(texts.common.templateMode),

    // Template code is required when templated.
    templateCode: Yup.string()
        .when('templated', (other: boolean, schema: Yup.StringSchema) =>
            (other ? schema.requiredI18n() : schema),
        )
        .label(texts.common.templateCode),

    // Subject is required when not templated.
    preformatted: Yup.object()
        .when('templated', (other: boolean, schema: Yup.ObjectSchema<any>) =>
            (other ? schema : schema.shape({ subject: Yup.object().label(texts.common.subject).atLeastOneStringI18n() })),
        )
        .label(texts.common.formatting),
});

export const PublishDialog = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const publishing = usePublish(x => x.publishing);
    const publishingError = usePublish(x => x.publishingError);
    const dialogOpen = usePublish(x => x.dialogOpen);
    const dialogValues = usePublish(x => x.dialogValues || {});
    const wasPublishing = usePrevious(publishing);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);

    React.useEffect(() => {
        if (wasPublishing && !publishing && !publishingError) {
            toast.info(texts.publish.success);
        }
    }, [publishing, publishingError, wasPublishing]);

    const doCloseForm = React.useCallback(() => {
        dispatch(togglePublishDialog({ open: false }));
    }, []);

    const doPublish = React.useCallback((params: PublishDto) => {
        dispatch(publishAsync({ appId, params }));
    }, [appId]);

    const initialValues: any = dialogValues || {};

    return (
        <Modal isOpen={dialogOpen} size='lg' backdrop={false} toggle={doCloseForm}>
            <Formik<PublishDto> initialValues={initialValues} onSubmit={doPublish} enableReinitialize validationSchema={FormSchema}>
                {({ handleSubmit, values }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {texts.publish.header}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset disabled={publishing}>
                                <Forms.Text name='topic'
                                    label={texts.common.topic} />
                            </fieldset>

                            <Forms.Boolean name='templated'
                                label={texts.common.templateMode} />

                            {values['templated'] ? (
                                <TemplateInput name='templateCode'
                                    label={texts.common.templateCode} />
                            ) : (
                                <NotificationsForm.Formatting
                                    onLanguageSelect={setLanguage}
                                    language={language}
                                    languages={appLanguages}
                                    field='preformatted' disabled={publishing} />
                            )}

                            <hr />

                            <Forms.Boolean name='test'
                                label={texts.integrations.test} />

                            <hr />

                            <Forms.Boolean name='silent'
                                label={texts.common.silent} />

                            <Forms.TextArea name='data'
                                label={texts.common.data} />

                            <hr />

                            <Forms.Number name='timeToLiveInSeconds'
                                label={texts.common.timeToLive} min={0} max={2419200} />

                            <hr />

                            <NotificationsForm.Settings
                                field='settings' disabled={publishing} />

                            <FormError error={publishingError} />
                        </ModalBody>
                        <ModalFooter className='justify-content-between' disabled={publishing}>
                            <Button type='button' color='' onClick={doCloseForm}>
                                {texts.common.cancel}
                            </Button>
                            <Button type='submit' color='success' disabled={publishing}>
                                <Loader light small visible={publishing} /> {texts.common.publish}
                            </Button>
                        </ModalFooter>
                    </Form>
                )}
            </Formik>
        </Modal>
    );
};
