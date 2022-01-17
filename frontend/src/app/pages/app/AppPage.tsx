/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useLocation, useRouteMatch } from 'react-router';
import { match, NavLink } from 'react-router-dom';
import { Dropdown, DropdownMenu, DropdownToggle, Nav, NavItem, NavLink as NavItemLink } from 'reactstrap';
import { combineUrl, Icon } from '@app/framework';
import { selectApp, togglePublishDialog, useApp, useApps } from '@app/state';
import { texts } from '@app/texts';
import { MessagingTemplatePage } from '../messaging-templates/MessagingTemplatePage';
import { MessagingTemplatesPage } from '../messaging-templates/MessagingTemplatesPage';
import { SmsTemplatePage } from '../sms-templates/SmsTemplatePage';
import { SmsTemplatesPage } from '../sms-templates/SmsTemplatesPage';
import { EmailTemplatePage } from './../email-templates/EmailTemplatePage';
import { EmailTemplatesPage } from './../email-templates/EmailTemplatesPage';
import { EventsPage } from './../events/EventsPage';
import { IntegrationsPage } from './../integrations/IntegrationsPage';
import { LogPage } from './../log/LogPage';
import { MediaPage } from './../media/MediaPage';
import { PublishDialog } from './../publish/PublishDialog';
import { TemplatesPage } from './../templates/TemplatesPage';
import { UserPage } from './../user/UserPage';
import { UsersPage } from './../users/UsersPage';
import { AppDashboardPage } from './AppDashboardPage';
import { AppSettingsPage } from './AppSettingsPage';

const DesignItem = ({ match, path }: { match: match<{}>; path: string }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const doToggle = React.useCallback(() => {
        setIsOpen(prev => !prev);
    }, []);

    const doClose = React.useCallback(() => {
        setIsOpen(false);
    }, []);

    const urlToEmailTemplates = combineUrl(match.url, 'email-templates');
    const urlToMedia = combineUrl(match.url, 'media');
    const urlToMessagingTemplates = combineUrl(match.url, 'messaging-templates');
    const urlToSmsTemplates = combineUrl(match.url, 'sms-templates');
    const urlToTemplates = combineUrl(match.url, 'templates');

    const isActive =
        path.startsWith(urlToEmailTemplates) ||
        path.startsWith(urlToMedia) ||
        path.startsWith(urlToMessagingTemplates) ||
        path.startsWith(urlToSmsTemplates) ||
        path.startsWith(urlToTemplates);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={doToggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='create' /> <span>{texts.common.design}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToMedia} onClick={doClose}>
                    <Icon type='photo_size_select_actual' /> <span>{texts.media.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToTemplates} onClick={doClose}>
                    <Icon type='file_copy' /> <span>{texts.templates.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToSmsTemplates} onClick={doClose}>
                    <Icon type='sms' /> <span>{texts.smsTemplates.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToEmailTemplates} onClick={doClose}>
                    <Icon type='mail_outline' /> <span>{texts.emailTemplates.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToMessagingTemplates} onClick={doClose}>
                    <Icon type='messaging' /> <span>{texts.messagingTemplates.header}</span>
                </NavLink>
            </DropdownMenu>
        </Dropdown>
    );
};

const MoreItem = ({ match, path }: { match: match<{}>; path: string }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const doToggle = React.useCallback(() => {
        setIsOpen(prev => !prev);
    }, []);

    const doClose = React.useCallback(() => {
        setIsOpen(false);
    }, []);

    const urlToLog = combineUrl(match.url, 'log');
    const urlToIntegrations = combineUrl(match.url, 'integrations');
    const urlToSettings = combineUrl(match.url, 'settings');

    const isActive =
        path.startsWith(urlToLog) ||
        path.startsWith(urlToIntegrations) ||
        path.startsWith(urlToSettings);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={doToggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='settings' /> <span>{texts.common.more}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToIntegrations} onClick={doClose}>
                    <Icon type='extension' /> <span>{texts.integrations.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToSettings} onClick={doClose}>
                    <Icon type='settings' /> <span>{texts.common.settings}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={urlToLog} onClick={doClose}>
                    <Icon type='history' /> <span>{texts.log.header}</span>
                </NavLink>
            </DropdownMenu>
        </Dropdown>
    );
};

export const AppPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const location = useLocation();
    const app = useApp();
    const appId = match.params['appId'];
    const loading = useApps(x => x.apps.isLoading);
    const [appSelected, setAppSelected] = React.useState(false);

    React.useEffect(() => {
        dispatch(selectApp({ appId }));

        setAppSelected(true);
    }, [dispatch, appId]);

    const doPublish = React.useCallback(() => {
        dispatch(togglePublishDialog({ open: true }));
    }, [dispatch]);

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
                    <NavLink activeClassName='active' className='nav-link' to={match.url} exact>
                        <Icon type='dashboard' /> <span>{texts.common.dashboard}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink activeClassName='active' className='nav-link' to={combineUrl(match.url, 'users')}>
                        <Icon type='person' /> <span>{texts.users.header}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink activeClassName='active' className='nav-link' to={combineUrl(match.url, 'events')}>
                        <Icon type='message' /> <span>{texts.events.header}</span>
                    </NavLink>
                </NavItem>

                <DesignItem match={match} path={location.pathname} />

                <MoreItem match={match} path={location.pathname} />

                <NavItem className='nav-publish'>
                    <NavItemLink className='nav-link' onClick={doPublish}>
                        <Icon type='send' /> <span>{texts.publish.header}</span>
                    </NavItemLink>
                </NavItem>
            </Nav>

            <Switch>
                <Route path={combineUrl(match.url, 'users/:userId/')}>
                    <UserPage />
                </Route>

                <Route path={combineUrl(match.url, 'users/')} exact>
                    <UsersPage />
                </Route>

                <Route path={combineUrl(match.url, 'events/')} exact>
                    <EventsPage />
                </Route>

                <Route path={combineUrl(match.url, 'templates/')} exact>
                    <TemplatesPage />
                </Route>

                <Route path={combineUrl(match.url, 'media/')} exact>
                    <MediaPage />
                </Route>

                <Route path={combineUrl(match.url, 'email-templates/')} exact>
                    <EmailTemplatesPage />
                </Route>

                <Route path={combineUrl(match.url, 'email-templates/:templateId/')} exact>
                    <EmailTemplatePage />
                </Route>

                <Route path={combineUrl(match.url, 'sms-templates/')} exact>
                    <SmsTemplatesPage />
                </Route>

                <Route path={combineUrl(match.url, 'sms-templates/:templateId/')} exact>
                    <SmsTemplatePage />
                </Route>

                <Route path={combineUrl(match.url, 'messaging-templates/')} exact>
                    <MessagingTemplatesPage />
                </Route>

                <Route path={combineUrl(match.url, 'messaging-templates/:templateId/')} exact>
                    <MessagingTemplatePage />
                </Route>

                <Route path={combineUrl(match.url, 'log/')} exact>
                    <LogPage />
                </Route>

                <Route path={combineUrl(match.url, 'integrations/')} exact>
                    <IntegrationsPage />
                </Route>

                <Route path={combineUrl(match.url, 'settings/')} exact>
                    <AppSettingsPage />
                </Route>

                <Route render={() =>
                    <AppDashboardPage />
                } />
            </Switch>

            <PublishDialog />
        </main>
    );
};
