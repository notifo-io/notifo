/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { yupResolver } from '@hookform/resolvers/yup';
import classNames from 'classnames';
import * as React from 'react';
import { FormProvider, useController, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, ButtonGroup, Col, Form, Label, Row } from 'reactstrap';
import * as Yup from 'yup';
import { Icon, Loader, useBooleanObj, useEventCallback } from '@app/framework';
import { EmailTemplateDto, MjmlSchema } from '@app/service';
import { Forms } from '@app/shared/components';
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
    const schema = useEmailTemplates(x => x.schema);
    const showHtml = useBooleanObj(true);
    const updateDialog = useBooleanObj();
    const updatingLanguage = useEmailTemplates(x => x.updatingLanguage);
    const updatingLanguageError = useEmailTemplates(x => x.updatingLanguageError);
    const disabled = updatingLanguage || deletingLanguage;

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

    const doCreate = useEventCallback(() => {
        dispatch(createEmailTemplateLanguage({ appId, id, language }));
    });

    const doDelete = useEventCallback(() => {
        dispatch(deleteEmailTemplateLanguage({ appId, id, language }));
    });

    const doUpdate = useEventCallback((params: EmailTemplateDto) => {
        dispatch(updateEmailTemplateLanguage({ appId, id, language, template: params }));
    });

    const form = useForm<EmailTemplateDto>({ resolver: yupResolver(FormSchema), defaultValues: template, mode: 'onChange' });

    React.useEffect(() => {
        form.reset(template);
    }, [form, template]);

    return template ? (
        <>
            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doUpdate)}>
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

                        <BodyHtml appId={appId} visible={showHtml.value} schema={schema} />
                        <BodyText appId={appId} visible={!showHtml.value} />
                    </div>
                </Form>

                {updateDialog.value &&
                    <EmailTemplateMoreDialog onClose={updateDialog.off} />
                }
            </FormProvider>
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

const BodyText = ({ visible, ...other }: { appId: string; visible: boolean }) => {
    const { field } = useController({ name: 'bodyText' });

    return (
        <>
            <Forms.Error name='bodyText' />

            <div className={classNames('email-body', { hidden: !visible })}>
                <EmailTextEditor {...other} {...field} />
            </div>
        </>
    );
};

const BodyHtml = ({ visible, ...other }: { appId: string; visible: boolean; schema?: MjmlSchema }) => {
    const { field } = useController({ name: 'bodyHtml' });

    return (
        <>
            <Forms.Error name='bodyHtml' />

            <div className={classNames('email-body', { hidden: !visible })}>
                <EmailHtmlEditor {...other} {...field} />
            </div>
        </>
    );
};