/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import { Formik, useFormikContext } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, ButtonGroup, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';
import { FormControlError, Icon, Loader, useBooleanObj, useEventCallback } from '@app/framework';
import { EmailTemplateDto } from '@app/service';
import { Forms, useFieldNew } from '@app/shared/components';
import { createEmailTemplateLanguage, deleteEmailTemplateLanguage, updateEmailTemplateLanguage, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import { EmailTemplateMoreDialog } from './EmailTemplateMoreDialog';
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
    const showHtml = useBooleanObj();
    const updateDialog = useBooleanObj();
    const updatingLanguage = useEmailTemplates(x => x.updatingLanguage);
    const updatingLanguageError = useEmailTemplates(x => x.updatingLanguageError);
    const [templateCopy, setTemplateCopy] = React.useState(template);

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

    const doCreate = useEventCallback(() => {
        dispatch(createEmailTemplateLanguage({ appId, id, language }));
    });

    const doDelete = useEventCallback(() => {
        dispatch(deleteEmailTemplateLanguage({ appId, id, language }));
    });

    const doUpdate = useEventCallback((params: EmailTemplateDto) => {
        const template = {
            ...params,
            fromEmail: templateCopy?.fromEmail,
            fromName: templateCopy?.fromName,
            kind: templateCopy?.kind,
        };

        dispatch(updateEmailTemplateLanguage({ appId, id, language, template }));
    });

    const disabled = updatingLanguage || deletingLanguage;

    return templateCopy ? (
        <>
            <Formik<EmailTemplateDto> initialValues={templateCopy} enableReinitialize onSubmit={doUpdate} validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <div className='email-container'>
                            <div className='email-menu'>
                                <Row className='align-items-center'>
                                    <Col xs='auto'>
                                        <ButtonGroup>
                                            <Button color='secondary' className='btn-flat' outline={!showHtml.value} onClick={showHtml.on}>
                                                {texts.common.html}
                                            </Button>
                                            <Button color='secondary' className='btn-flat' outline={showHtml.value} onClick={showHtml.off}>
                                                {texts.common.text}
                                            </Button>
                                        </ButtonGroup>
                                    </Col>
                                    <Col>
                                        <Button color='blank' onClick={updateDialog.on}>
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

                            <BodyHtml appId={appId} kind={template?.kind} visible={showHtml.value} />
                            <BodyText appId={appId} kind={template?.kind} visible={!showHtml.value} />
                        </div>
                    </Form>
                )}
            </Formik>

            {updateDialog.value &&
                <EmailTemplateMoreDialog template={templateCopy} onClose={updateDialog.off} />
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

const BodyText = ({ visible, ...other }: { appId: string; kind: string | undefined; visible: boolean }) => {
    const field = useFieldContext('bodyText', visible);

    return (
        <>
            <FormControlError error={field.meta.error} touched={field.meta.touched} submitCount={field.submitCount} />

            <div className={field.className}>
                <EmailTextEditor initialValue={field.value} {...other}
                    onChange={field.onChange}
                    onBlur={field.onBlur} />
            </div>
        </>
    );
};

const BodyHtml = ({ visible, ...other }: { appId: string; kind: string | undefined; visible: boolean }) => {
    const field = useFieldContext('bodyHtml', visible);

    return (
        <>
            <FormControlError error={field.meta.error} touched={field.meta.touched} submitCount={field.submitCount} />

            <div className={field.className}>
                <EmailHtmlEditor initialValue={field.value} {...other}
                    onChange={field.onChange}
                    onBlur={field.onBlur} />
            </div>
        </>
    );
};

function useFieldContext(name: string, visible: boolean) {
    const { initialValues, submitCount } = useFormikContext<EmailTemplateDto>();
    const [, meta, helpers] = useFieldNew('bodyHtml');

    const doTouch = useEventCallback(() => {
        helpers.setTouched(true);
    });

    return {
        className: classNames('email-body', { hidden: !visible }),
        meta,
        onBlur: doTouch,
        onChange: helpers.setValue,
        submitCount,
        value: initialValues[name],
    };
}
