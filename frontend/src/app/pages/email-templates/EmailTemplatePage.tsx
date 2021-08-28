/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon, Loader, Toggle, useStateWithRef } from '@app/framework';
import { LanguageSelector } from '@app/framework/react/LanguageSelector';
import { loadEmailTemplate, updateEmailTemplate, useApp, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { NavLink } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Col, Form, Input, Label, Row } from 'reactstrap';
import { EmailTemplate } from './EmailTemplate';

export const EmailTemplatePage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const appName = app.name;
    const loadingTemplate = useEmailTemplates(x => x.loadingTemplate);
    const loadingTemplateError = useEmailTemplates(x => x.loadingTemplateError);
    const template = useEmailTemplates(x => x.template);
    const templateId = match.params['templateId'];
    const updating = useEmailTemplates(x => x.updating);
    const updatingError = useEmailTemplates(x => x.updatingError);
    const upserting = useEmailTemplates(x => x.creatingLanguage || x.deletingLanguage || x.updatingLanguage);
    const [name, setName, nameRef] = useStateWithRef('');
    const [language, setLanguage] = React.useState(appLanguages[0]);

    React.useEffect(() => {
        dispatch(loadEmailTemplate({ appId, id: templateId }));
    }, [appId, templateId]);

    React.useEffect(() => {
        if (loadingTemplateError) {
            toast.error(loadingTemplateError.response);
        }
    }, [loadingTemplateError]);

    React.useEffect(() => {
        if (updatingError) {
            toast.error(updatingError.response);
        }
    }, [updatingError]);

    React.useEffect(() => {
        if (template) {
            setName(template.name || '');
        }
    }, [template]);

    const doSetName = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setName(event.target.value);
    }, []);

    const doUpdatePrimary = React.useCallback((primary: boolean) => {
        dispatch(updateEmailTemplate({ appId, id: templateId, update: { primary } }));
    }, [appId, templateId]);

    const doUpdateName = React.useCallback((event: React.FormEvent<HTMLFormElement>) => {
        dispatch(updateEmailTemplate({ appId, id: templateId, update: { name: nameRef.current } }));

        event.preventDefault();
    }, [appId, templateId]);

    const doSetLanguage = (language: string) => {
        if (!loadingTemplate && !upserting) {
            setLanguage(language);
        }
    };

    return (
        <>
            <div className='email-header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <Row noGutters className='align-items-center'>
                            <Col xs='auto'>
                                <NavLink className='btn btn-back btn-flat' to='./'>
                                    <Icon type='keyboard_arrow_left' />
                                </NavLink>
                            </Col>
                            <Col xs='auto'>
                                <Form onSubmit={doUpdateName}>
                                    <Input name={name} disabled={updating} onChange={doSetName} placeholder={texts.emailTemplates.name} />
                                </Form>
                            </Col>
                        </Row>
                    </Col>
                    <Col>
                        <Loader visible={loadingTemplate} />
                    </Col>

                    {template &&
                        <>
                            <Col xs='auto'>
                                <div className='btn btn-toggle btn-flat'>
                                    <Label>
                                        {texts.common.primary}
                                    </Label>

                                    <Toggle disabled={updating} value={template?.primary} onChange={doUpdatePrimary} />
                                </div>
                            </Col>
                            <Col xs='auto'>
                                <LanguageSelector
                                    color='simple'
                                    size='md'
                                    language={language}
                                    languages={appLanguages}
                                    onSelect={doSetLanguage} />
                            </Col>
                        </>
                    }
                </Row>
            </div>

            {template && !loadingTemplate &&
                <EmailTemplate
                    appId={appId}
                    appName={appName}
                    language={language}
                    template={template.languages[language]}
                    templateId={template.id} />
            }
        </>
    );
};
