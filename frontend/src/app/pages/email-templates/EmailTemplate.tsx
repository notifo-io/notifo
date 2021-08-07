/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormControlError, Forms, Icon, Loader } from '@app/framework';
import { EmailTemplateDto } from '@app/service';
import { EmailHtmlEditor, EmailTextEditor } from '@app/shared/components';
import { createEmailTemplateLanguage, deleteEmailTemplateLanguage, updateEmailTemplateLanguage, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import { Formik, useField, useFormikContext } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, ButtonGroup, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';

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
        appName,
        templateId: id,
        language,
        template,
    } = props;

    const dispatch = useDispatch();
    const creating = useEmailTemplates(x => x.creating);
    const creatingError = useEmailTemplates(x => x.creatingError);
    const deleting = useEmailTemplates(x => x.deleting);
    const deletingError = useEmailTemplates(x => x.deletingError);
    const updating = useEmailTemplates(x => x.updating);
    const updatingError = useEmailTemplates(x => x.updatingError);
    const [showHtml, setShowHtml] = React.useState(true);

    React.useEffect(() => {
        if (creatingError) {
            toast.error(creatingError.response);
        }
    }, [creatingError]);

    React.useEffect(() => {
        if (updatingError) {
            toast.error(updatingError.response);
        }
    }, [updatingError]);

    React.useEffect(() => {
        if (deletingError) {
            toast.error(deletingError.response);
        }
    }, [deletingError]);

    const doShowhtml = React.useCallback(() => {
        setShowHtml(true);
    }, []);

    const doShowText = React.useCallback(() => {
        setShowHtml(false);
    }, []);

    const doCreate = React.useCallback(() => {
        dispatch(createEmailTemplateLanguage({ appId, id, language }));
    }, [appId, id, language]);

    const doUpdate = React.useCallback((template: EmailTemplateDto) => {
        dispatch(updateEmailTemplateLanguage({ appId, id, language, template }));
    }, [appId, id, language]);

    const doDelete = React.useCallback(() => {
        dispatch(deleteEmailTemplateLanguage({ appId, id, language }));
    }, [appId, id, language]);

    const disabled = updating || deleting;

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
                                        <Loader light small visible={updating} /> {texts.common.save}
                                    </Button>
                                    <Button color='danger' disabled={disabled} type='button' onClick={doDelete}>
                                        <Loader light small visible={deleting} /> <Icon type='delete' />
                                    </Button>
                                </Col>
                            </Row>
                        </div>

                        <div className='email-subject' >
                            <Forms.Text name='subject' label={texts.common.subject} />
                        </div>

                        <BodyHtml appName={appName} visible={showHtml} />
                        <BodyText appName={appName} visible={!showHtml} />
                    </div>
                </Form>
            )}
        </Formik>
    ) : (
        <>
            <div className='email-none'>
                <Label>{texts.emailTemplates.notFound}</Label>

                <Button size='lg' color='success' disabled={creating} onClick={doCreate}>
                    <Loader light small visible={creating} /> <Icon type='add' /> {texts.emailTemplates.notFoundButton}
                </Button>
            </div>
        </>
    );
};

export const BodyText = ({ appName, visible }: { appName: string; visible: boolean }) => {
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
                <EmailTextEditor value={initialValues.bodyText} appName={appName}
                    onChange={helpers.setValue}
                    onBlur={doTouch} />
            </div>
        </>
    );
};

export const BodyHtml = ({ appName, visible }: { appName: string; visible: boolean }) => {
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
                <EmailHtmlEditor value={initialValues.bodyHtml} appName={appName}
                    onChange={helpers.setValue}
                    onBlur={doTouch} />
            </div>
        </>
    );
};
