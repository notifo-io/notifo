/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormControlError, Forms, Icon, Loader, useDialog, useFieldNew } from '@app/framework';
import { EmailTemplateDto } from '@app/service';
import { createEmailTemplateLanguage, deleteEmailTemplateLanguage, updateEmailTemplateLanguage, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import { Formik, useFormikContext } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, ButtonGroup, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';
import { EmailHtmlEditor } from './editor/EmailHtmlEditor';
import { EmailTextEditor } from './editor/EmailTextEditor';
import { EmailTemplateMoreDialog } from './EmailTemplateMoreDialog';

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
    const moreDialog = useDialog();
    const updatingLanguage = useEmailTemplates(x => x.updatingLanguage);
    const updatingLanguageError = useEmailTemplates(x => x.updatingLanguageError);
    const [templateCopy, setTemplateCopy] = React.useState(template);
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

    React.useEffect(() => {
        if (template) {
            setTemplateCopy({ ...template });
        } else {
            undefined;
        }
    }, [template]);

    const doShowhtml = React.useCallback(() => {
        setShowHtml(true);
    }, []);

    const doShowText = React.useCallback(() => {
        setShowHtml(false);
    }, []);

    const doCreate = React.useCallback(() => {
        dispatch(createEmailTemplateLanguage({ appId, id, language }));
    }, [dispatch, appId, id, language]);

    const doDelete = React.useCallback(() => {
        dispatch(deleteEmailTemplateLanguage({ appId, id, language }));
    }, [dispatch, appId, id, language]);

    const doUpdate = React.useCallback((params: EmailTemplateDto) => {
        const template = {
            ...params,
            fromEmail: templateCopy?.fromEmail,
            fromName: templateCopy?.fromName,
        };

        dispatch(updateEmailTemplateLanguage({ appId, id, language, template }));
    }, [dispatch, appId, id, language, templateCopy]);

    const disabled = updatingLanguage || deletingLanguage;

    return templateCopy ? (
        <>
            <Formik<EmailTemplateDto> initialValues={templateCopy} onSubmit={doUpdate} validationSchema={FormSchema} enableReinitialize>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <div className='email-container'>
                            <div className='email-menu'>
                                <Row className='align-items-center'>
                                    <Col xs='auto'>
                                        <ButtonGroup>
                                            <Button color='secondary' className='btn-flat' outline={!showHtml} onClick={doShowhtml}>
                                                {texts.common.html}
                                            </Button>
                                            <Button color='secondary' className='btn-flat' outline={showHtml} onClick={doShowText}>
                                                {texts.common.text}
                                            </Button>
                                        </ButtonGroup>
                                    </Col>
                                    <Col>
                                        <Button color='blank' onClick={moreDialog.open}>
                                            <Icon className='text-lg' type='create' />
                                        </Button>
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
                                <Forms.Text name='subject' label={texts.common.subject} vertical />
                            </div>

                            <BodyHtml appId={appId} visible={showHtml} />
                            <BodyText appId={appId} visible={!showHtml} />
                        </div>
                    </Form>
                )}
            </Formik>

            {moreDialog.isOpen &&
                <EmailTemplateMoreDialog template={templateCopy} onClose={moreDialog.close} />
            }
        </>
    ) : (
        <div className='empty-button'>
            <Label>{texts.emailTemplates.notFoundLanguage}</Label>

            <Button size='lg' color='success' disabled={creatingLanguage} onClick={doCreate}>
                <Loader light small visible={creatingLanguage} /> <Icon type='add' /> {texts.emailTemplates.notFoundButton}
            </Button>
        </div>
    );
};

const BodyText = ({ appId, visible }: { appId: string; visible: boolean }) => {
    const field = useFieldContext('bodyText', visible);

    return (
        <>
            <FormControlError error={field.meta.error} touched={field.meta.touched} submitCount={field.submitCount} />

            <div className={field.className}>
                <EmailTextEditor initialValue={field.value} appId={appId}
                    onChange={field.onChange}
                    onBlur={field.onBlur} />
            </div>
        </>
    );
};

const BodyHtml = ({ appId, visible }: { appId: string; visible: boolean }) => {
    const field = useFieldContext('bodyHtml', visible);

    return (
        <>
            <FormControlError error={field.meta.error} touched={field.meta.touched} submitCount={field.submitCount} />

            <div className={field.className}>
                <EmailHtmlEditor initialValue={field.value} appId={appId}
                    onChange={field.onChange}
                    onBlur={field.onBlur} />
            </div>
        </>
    );
};

function useFieldContext(name: string, visible: boolean) {
    const { initialValues, submitCount } = useFormikContext<EmailTemplateDto>();
    const [, meta, helpers] = useFieldNew('bodyHtml');

    const doTouch = React.useCallback(() => {
        helpers.setTouched(true);
    }, [helpers]);

    const clazz = !visible ? 'email-body hidden' : 'email-body';

    return {
        className: clazz,
        meta,
        onBlur: doTouch,
        onChange: helpers.setValue,
        submitCount,
        value: initialValues[name],
    };
}
