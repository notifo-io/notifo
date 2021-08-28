/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader, usePrevious } from '@app/framework';
import { PublishDto } from '@app/service';
import { NotificationsForm, TemplateInput } from '@app/shared/components';
import { publish, togglePublishDialog, useApp, usePublish } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader, Nav, NavItem, NavLink } from 'reactstrap';
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
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const publishing = usePublish(x => x.publishing);
    const publishingError = usePublish(x => x.publishingError);
    const dialogOpen = usePublish(x => x.dialogOpen);
    const dialogValues = usePublish(x => x.dialogValues || {});
    const wasPublishing = usePrevious(publishing);
    const [tab, setTab] = React.useState(0);
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
        dispatch(publish({ appId, params }));
    }, [appId]);

    const initialValues: any = dialogValues || {};

    return (
        <Modal isOpen={dialogOpen} size='lg' backdrop={false} toggle={doCloseForm}>
            <Formik<PublishDto> initialValues={initialValues} onSubmit={doPublish} enableReinitialize validationSchema={FormSchema}>
                {({ handleSubmit, values }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            <Nav className='nav-tabs2'>
                                <NavItem>
                                    <NavLink onClick={() => setTab(0)} active={tab === 0}>{texts.common.publish}</NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink onClick={() => setTab(1)} active={tab === 1}>{texts.common.channels}</NavLink>
                                </NavItem>
                            </Nav>
                        </ModalHeader>

                        <ModalBody>
                            {tab === 0 ? (
                                <fieldset disabled={publishing}>
                                    <Forms.Text name='topic'
                                        label={texts.common.topic} />

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

                                    <Forms.Textarea name='data'
                                        label={texts.common.data} />

                                    <hr />

                                    <Forms.Number name='timeToLiveInSeconds'
                                        label={texts.common.timeToLive} min={0} max={2419200} />
                                </fieldset>
                            ) : (
                                <NotificationsForm.Settings field='settings' disabled={publishing} />
                            )}

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
