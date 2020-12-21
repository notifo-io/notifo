/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Loader } from '@app/framework';
import { getApp, loadEmailTemplatesAsync, useApps, useEmailTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Col, Nav, NavItem, NavLink, Row } from 'reactstrap';
import { EmailTemplate } from './EmailTemplate';

export const EmailsPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const appName = app.name;
    const emailTemplates = useEmailTemplates(x => x.emailTemplates);
    const loading = useEmailTemplates(x => x.loading);
    const loadingError = useEmailTemplates(x => x.loadingError);
    const upserting = useEmailTemplates(x => x.creating || x.deleting || x.updating);
    const [language, setLanguage] = React.useState(appLanguages[0]);

    React.useEffect(() => {
        dispatch(loadEmailTemplatesAsync({ appId }));
    }, [appId]);

    React.useEffect(() => {
        if (loadingError) {
            toast.error(loadingError.response);
        }
    }, [loadingError]);

    const doSetLanguage = (language: string) => {
        if (!loading && !upserting) {
            setLanguage(language);
        }
    };

    return (
        <>
            <div className='email-header'>
                <Row className='align-items-center'>
                    <Col xs='auto'>
                        <h2 className='truncate'>{texts.emails.header}</h2>
                    </Col>
                    <Col>
                        <Loader visible={loading} />
                    </Col>
                    <Col xs='auto'>
                        <Nav pills>
                            {appLanguages.map(x => (
                                <NavItem key={x}>
                                    <NavLink  onClick={() => doSetLanguage(x)} active={x === language}>
                                        {x}
                                    </NavLink>
                                </NavItem>
                            ))}
                        </Nav>
                    </Col>
                </Row>
            </div>

            {emailTemplates && !loading &&
                <EmailTemplate
                    appId={appId}
                    appName={appName}
                    language={language}
                    template={emailTemplates[language]} />
            }
        </>
    );
};
