/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormControlError, Forms, Icon, Loader } from '@app/framework';
import { EmailTemplateDto } from '@app/service';
import { createEmailTemplateLanguage, deleteEmailTemplateLanguage, updateEmailTemplateLanguage, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import { Formik, useField, useFormikContext } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, ButtonGroup, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';
import { EmailHtmlEditor } from './editor/EmailHtmlEditor';
import { EmailTextEditor } from './editor/EmailTextEditor';

const FormSchema = Yup.object().shape({
    // Required html body
    bodyHtml: Yup.string()
        .label(texts.emailTemplates.bodyHtml).required(texts.emailTemplates.bodyHtmlValid),

    // Required text body
    bodyText: Yup.string()
        .label(texts.emailTemplates.bodyText).required(texts.emailTemplates.bodyTextValid),

    // Required text body
    subject: Yup.string()
        .label(texts.common.subject).required(texts.emailTemplates.subjectValid),
});

export interface EmailTemplateProps {
    // The app id.
    appId: string;

    // The app name.
    appName: string;

    // The template id.
    templateId: string;

    // The template language.
    language: string;

    // The template if found.
    template?: EmailTemplateDto;
}

export const EmailTemplate = (props: EmailTemplateProps) => {
    const {
        appId,
        templateId: id,
        language,
        template,
    } = props;

    const dispatch = useDispatch();
    const creatingLanguage = useEmailTemplates(x => x.creatingLanguage);
    const creatingLanguageError = useEmailTemplates(x => x.creatingLanguageError);
    const deletingLanguage = useEmailTemplates(x => x.deletingLanguage);
    const deletingLanguageError = useEmailTemplates(x => x.deletingLanguageError);
    const updatingLanguage = useEmailTemplates(x => x.updatingLanguage);
    const updatingLanguageError = useEmailTemplates(x => x.updatingLanguageError);
    const [showHtml, setShowHtml] = React.useState(true);

    React.useEffect(() => {
        if (creatingLanguageError) {
            toast.error(creatingLanguageError.response);
        }
    }, [creatingLanguageError]);

    React.useEffect(() => {
        if (updatingLanguageError) {
            toast.error(updatingLanguageError.response);
        }
    }, [updatingLanguageError]);

    React.useEffect(() => {
        if (deletingLanguageError) {
            toast.error(deletingLanguageError.response);
        }
    }, [deletingLanguageError]);

    const doShowhtml = React.useCallback(() => {
        setShowHtml(true);
    }, []);

    const doShowText = React.useCallback(() => {
        setShowHtml(false);
    }, []);

    const doCreate = React.useCallback(() => {
        dispatch(createEmailTemplateLanguage({ appId, id, language }));
    }, [dispatch, appId, id, language]);

    const doUpdate = React.useCallback((template: EmailTemplateDto) => {
        dispatch(updateEmailTemplateLanguage({ appId, id, language, template }));
    }, [dispatch, appId, id, language]);

    const doDelete = React.useCallback(() => {
        dispatch(deleteEmailTemplateLanguage({ appId, id, language }));
    }, [dispatch, appId, id, language]);

    const disabled = updatingLanguage || deletingLanguage;

    return template ? (
        <Formik<EmailTemplateDto> initialValues={template} onSubmit={doUpdate} validationSchema={FormSchema} enableReinitialize>
            {({ handleSubmit }) => (
                <Form onSubmit={handleSubmit}>
                    <div className='email-container'>
                        <div className='email-menu'>
                            <Row className='align-items-center'>
                                <Col>
                                    <ButtonGroup>
                                        <Button color='secondary' className='btn-flat' outline={!showHtml} onClick={doShowhtml}>
                                            {texts.common.html}
                                        </Button>
                                        <Button color='secondary' className='btn-flat' outline={showHtml} onClick={doShowText}>
                                            {texts.common.text}
                                        </Button>
                                    </ButtonGroup>
                                </Col>
                                <Col xs='auto'>
                                    <Button color='primary' disabled={disabled} type='submit'>
                                        <Loader light small visible={updatingLanguage} /> {texts.common.save}
                                    </Button>
                                    <Button color='danger' disabled={disabled} type='button' onClick={doDelete}>
                                        <Loader light small visible={deletingLanguage} /> <Icon type='delete' />
                                    </Button>
                                </Col>
                            </Row>
                        </div>

                        <div className='email-subject' >
                            <Forms.Text name='subject' label={texts.common.subject} />
                        </div>

                        <BodyHtml appId={appId} visible={showHtml} />
                        <BodyText appId={appId} visible={!showHtml} />
                    </div>
                </Form>
            )}
        </Formik>
    ) : (
        <div className='empty-button'>
            <Label>{texts.emailTemplates.notFoundLanguage}</Label>

            <Button size='lg' color='success' disabled={creatingLanguage} onClick={doCreate}>
                <Loader light small visible={creatingLanguage} /> <Icon type='add' /> {texts.emailTemplates.notFoundButton}
            </Button>
        </div>
    );
};

export const BodyText = ({ appId, visible }: { appId: string; visible: boolean }) => {
    const { initialValues, submitCount } = useFormikContext<EmailTemplateDto>();
    const [, meta, helpers] = useField('bodyText');

    const doTouch = React.useCallback(() => {
        helpers.setTouched(true);
    }, [helpers]);

    const clazz = !visible ? 'email-body hidden' : 'email-body';

    return (
        <>
            <FormControlError error={meta.error} touched={meta.touched} submitCount={submitCount} />

            <div className={clazz}>
                <EmailTextEditor value={initialValues.bodyText} appId={appId}
                    onChange={helpers.setValue}
                    onBlur={doTouch} />
            </div>
        </>
    );
};

export const BodyHtml = ({ appId, visible }: { appId: string; visible: boolean }) => {
    const { initialValues, submitCount } = useFormikContext<EmailTemplateDto>();
    const [, meta, helpers] = useField('bodyHtml');

    const doTouch = React.useCallback(() => {
        helpers.setTouched(true);
    }, [helpers]);

    const clazz = !visible ? 'email-body hidden' : 'email-body';

    return (
        <>
            <FormControlError error={meta.error} touched={meta.touched} submitCount={submitCount} />

            <div className={clazz}>
                <EmailHtmlEditor value={initialValues.bodyHtml} appId={appId}
                    onChange={helpers.setValue}
                    onBlur={doTouch} />
            </div>
        </>
    );
};
