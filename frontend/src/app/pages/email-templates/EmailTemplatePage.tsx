/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Loader } from '@app/framework';
import { LanguageSelector } from '@app/framework/react/LanguageSelector';
import { getApp, loadEmailTemplate, useApps, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { toast } from 'react-toastify';
import { Col, Row } from 'reactstrap';
import { EmailTemplate } from './EmailTemplate';

export const EmailsPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const appName = app.name;
    const loadingTemplate = useEmailTemplates(x => x.loadingTemplate);
    const loadingTemplateError = useEmailTemplates(x => x.loadingTemplateError);
    const template = useEmailTemplates(x => x.template);
    const templateId = match.params['id'];
    const upserting = useEmailTemplates(x => x.creatingLanguage || x.deletingLanguage || x.updating || x.updatingLanguage);
    const [language, setLanguage] = React.useState(appLanguages[0]);

    React.useEffect(() => {
        dispatch(loadEmailTemplate({ appId, id: templateId }));
    }, [appId, templateId]);

    React.useEffect(() => {
        if (loadingTemplateError) {
            toast.error(loadingTemplateError.response);
        }
    }, [loadingTemplateError]);

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
                        <h2 className='truncate'>{texts.emailTemplates.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={loadingTemplate} />
                    </Col>
                    <Col xs='auto'>
                        <LanguageSelector
                            size='md'
                            language={language}
                            languages={appLanguages}
                            onSelect={doSetLanguage} />
                    </Col>
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
