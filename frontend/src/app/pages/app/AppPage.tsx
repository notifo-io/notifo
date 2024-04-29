/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Navigate, Route, Routes, useLocation, useParams, useResolvedPath } from 'react-router';
import { NavLink } from 'react-router-dom';
import { Dropdown, DropdownMenu, DropdownToggle, Nav, NavItem, NavLink as NavItemLink } from 'reactstrap';
import { Icon, useBoolean, useEventCallback } from '@app/framework';
import { selectApp, togglePublishDialog, useApp, useApps } from '@app/state';
import { texts } from '@app/texts';
import { EmailTemplatePage } from './../email-templates/EmailTemplatePage';
import { EmailTemplatesPage } from './../email-templates/EmailTemplatesPage';
import { EventsPage } from './../events/EventsPage';
import { IntegrationsPage } from './../integrations/IntegrationsPage';
import { LogPage } from './../log/LogPage';
import { MediaPage } from './../media/MediaPage';
import { MessagingTemplatePage } from './../messaging-templates/MessagingTemplatePage';
import { MessagingTemplatesPage } from './../messaging-templates/MessagingTemplatesPage';
import { PublishDialog } from './../publish/PublishDialog';
import { SmsTemplatePage } from './../sms-templates/SmsTemplatePage';
import { SmsTemplatesPage } from './../sms-templates/SmsTemplatesPage';
import { TemplatesPage } from './../templates/TemplatesPage';
import { TopicsPage } from './../topics/TopicsPage';
import { UserPage } from './../user/UserPage';
import { UsersPage } from './../users/UsersPage';
import { AppDashboardPage } from './AppDashboardPage';
import { AppSettingsPage } from './AppSettingsPage';

const NavDesign = () => {
    const [isOpen, setIsOpen] = useBoolean();
    const parent = useResolvedPath('').pathname;
    const path = useLocation().pathname;

    const isActive = React.useMemo(() => {
        let location = path;

        if (location.startsWith(parent)) {
            location = location.substring(parent.length);
        }

        const result =
            location.startsWith('/media') ||
            location.startsWith('/templates') ||
            location.startsWith('/sms-templates') ||
            location.startsWith('/mail-templates') ||
            location.startsWith('/messaging-templates');

        return result;
    }, [parent, path]);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={setIsOpen.toggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='create' /> <span>{texts.common.design}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink className='dropdown-item' to='media' onClick={setIsOpen.off}>
                    <Icon type='photo_size_select_actual' /> <span>{texts.media.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='templates' onClick={setIsOpen.off}>
                    <Icon type='file_copy' /> <span>{texts.templates.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='sms-templates' onClick={setIsOpen.off}>
                    <Icon type='sms' /> <span>{texts.smsTemplates.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='email-templates' onClick={setIsOpen.off}>
                    <Icon type='mail_outline' /> <span>{texts.emailTemplates.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='messaging-templates' onClick={setIsOpen.off}>
                    <Icon type='messaging' /> <span>{texts.messagingTemplates.header}</span>
                </NavLink>
            </DropdownMenu>
        </Dropdown>
    );
};

const NavMore = () => {
    const [isOpen, setIsOpen] = useBoolean();
    const parent = useResolvedPath('').pathname;
    const path = useLocation().pathname;

    const isActive = React.useMemo(() => {
        let location = path;

        if (location.startsWith(parent)) {
            location = location.substring(parent.length);
        }

        const result =
            location.startsWith('/integrations') ||
            location.startsWith('/topics') ||
            location.startsWith('/settings') ||
            location.startsWith('/log');

        return result;
    }, [parent, path]);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={setIsOpen.toggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='settings' /> <span>{texts.common.more}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink className='dropdown-item' to='integrations' onClick={setIsOpen.off}>
                    <Icon type='extension' /> <span>{texts.integrations.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='topics' onClick={setIsOpen.off}>
                    <Icon type='topic' /> <span>{texts.topics.header}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='settings' onClick={setIsOpen.off}>
                    <Icon type='settings' /> <span>{texts.common.settings}</span>
                </NavLink>
                <NavLink className='dropdown-item' to='log' onClick={setIsOpen.off}>
                    <Icon type='history' /> <span>{texts.log.header}</span>
                </NavLink>
            </DropdownMenu>
        </Dropdown>
    );
};

export const AppPage = () => {
    const dispatch = useDispatch<any>();
    const app = useApp();
    const appId = useParams().appId!;
    const loading = useApps(x => x.apps.isLoading);
    const [appSelected, setAppSelected] = React.useState(false);

    React.useEffect(() => {
        dispatch(selectApp({ appId }));

        setAppSelected(true);
    }, [dispatch, appId]);

    const doPublish = useEventCallback(() => {
        dispatch(togglePublishDialog({ open: true }));
    });

    if (loading || !appSelected) {
        return null;
    }

    if (!app) {
        return <>{texts.apps.notFound}</>;
    }

    return (
        <main>
            <Nav vertical className='sidebar'>
                <NavItem>
                    <NavLink className='nav-link' to='home'>
                        <Icon type='dashboard' /> <span>{texts.common.dashboard}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink className='nav-link' to='users'>
                        <Icon type='person' /> <span>{texts.users.header}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink className='nav-link' to='events'>
                        <Icon type='message' /> <span>{texts.events.header}</span>
                    </NavLink>
                </NavItem>

                <NavDesign />
                <NavMore />

                <NavItem className='nav-publish'>
                    <NavItemLink className='nav-link' onClick={doPublish}>
                        <Icon type='send' /> <span>{texts.publish.header}</span>
                    </NavItemLink>
                </NavItem>
            </Nav>

            <Routes>
                <Route path='users/:userId/*'
                    element={<UserPage />} />

                <Route path='users'
                    element={<UsersPage />} />

                <Route path='events'
                    element={<EventsPage />} />

                <Route path='templates'
                    element={<TemplatesPage />} />

                <Route path='media'
                    element={<MediaPage />} />

                <Route path='email-templates'
                    element={<EmailTemplatesPage />} />

                <Route path='email-templates/:templateId/'
                    element={<EmailTemplatePage />} />

                <Route path='sms-templates/'
                    element={<SmsTemplatesPage />} />

                <Route path='sms-templates/:templateId/'
                    element={<SmsTemplatePage />} />

                <Route path='sms-templates'
                    element={<SmsTemplatePage />} />

                <Route path='messaging-templates/:templateId/'
                    element={<MessagingTemplatePage />} />

                <Route path='messaging-templates'
                    element={<MessagingTemplatesPage />} />

                <Route path='log/:userId?'
                    element={<LogPage />} />

                <Route path='integrations'
                    element={<IntegrationsPage />} />

                <Route path='settings'
                    element={<AppSettingsPage />} />

                <Route path='topics'
                    element={<TopicsPage />} />

                <Route path='home'
                    element={<AppDashboardPage />} />

                <Route path='*'
                    element={<Navigate to='home' />} />
            </Routes>

            <PublishDialog />
        </main>
    );
};
