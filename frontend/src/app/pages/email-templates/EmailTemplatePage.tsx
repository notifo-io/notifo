/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useParams } from 'react-router';
import { NavLink } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Button, Col, Label, Row } from 'reactstrap';
import { Icon, Loader, Toggle, useBooleanObj, useEventCallback, useSimpleQuery } from '@app/framework';
import { LanguageSelector } from '@app/framework/react/LanguageSelector';
import { Clients, TemplatePropertyDto } from '@app/service';
import { TemplateProperties } from '@app/shared/components';
import { loadEmailTemplate, updateEmailTemplate, useApp, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import { EmailTemplate } from './EmailTemplate';
import { EmailTemplateName } from './EmailTemplateName';

export const EmailTemplatePage = () => {
    const dispatch = useDispatch<any>();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const appName = app.name;
    const loadingTemplate = useEmailTemplates(x => x.loadingTemplate);
    const loadingTemplateError = useEmailTemplates(x => x.loadingTemplateError);
    const sidebar = useBooleanObj();
    const template = useEmailTemplates(x => x.template);
    const templateId = useParams().templateId!;
    const updating = useEmailTemplates(x => x.updating);
    const updatingError = useEmailTemplates(x => x.updatingError);
    const upserting = useEmailTemplates(x => x.creatingLanguage || x.deletingLanguage || x.updatingLanguage);
    const [language, setLanguage] = React.useState(appLanguages[0]);

    const properties = useSimpleQuery<TemplatePropertyDto[]>({
        queryKey: [appId],
        queryFn: async abort => {
            const result = await Clients.SmsTemplates.getProperties(appId, abort);
            
            return result.items;
        },
        defaultValue: [],
    });

    React.useEffect(() => {
        dispatch(loadEmailTemplate({ appId, id: templateId }));
    }, [dispatch, appId, templateId]);

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

    const doUpdatePrimary = useEventCallback((primary: any) => {
        dispatch(updateEmailTemplate({ appId, id: templateId, update: { primary } }));
    });

    const doSetLanguage = (language: string) => {
        if (!loadingTemplate && !upserting) {
            setLanguage(language);
        }
    };

    return (
        <>
            <div className='email-header'>
                <Row className='align-items-center gap-2' noGutters>
                    <Col xs='auto'>
                        <Row noGutters className='align-items-center'>
                            <Col xs='auto'>
                                <NavLink className='btn btn-back btn-flat' to='./../'>
                                    <Icon type='keyboard_arrow_left' />
                                </NavLink>
                            </Col>
                            <Col xs='auto'>
                                <EmailTemplateName template={template} appId={appId} />
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

                    <Col xs='auto'>
                        <Button className='btn-flat' color='blank' onClick={sidebar.toggle}>
                            <Icon className='text-icon' type='help-circle' />
                        </Button>
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

            {sidebar.value &&
                <div className='email-properties slide-right'>
                    <Row>
                        <Col>
                            <h3>{texts.common.properties}</h3>
                        </Col>
                        <Col xs='auto'>
                            <Button className='btn-flat' color='blank' size='sm' onClick={sidebar.toggle}>
                                <Icon className='text-icon' type='clear' />
                            </Button>
                        </Col>
                    </Row>
                    <TemplateProperties properties={properties.value} />
                </div>
            }
        </>
    );
};
