/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon } from '@app/framework';
import { getApp, openPublishDialog, selectApp, useApps } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useLocation, useRouteMatch } from 'react-router';
import { match, NavLink } from 'react-router-dom';
import { Dropdown, DropdownMenu, DropdownToggle, Nav, NavItem, NavLink as NavItemLink } from 'reactstrap';
import { EmailsPage } from './../emails/EmailsPage';
import { EventsPage } from './../events/EventsPage';
import { LogPage } from './../log/LogPage';
import { MediaPage } from './../media/MediaPage';
import { PublishDialog } from './../publish/PublishDialog';
import { TemplatesPage } from './../templates/TemplatesPage';
import { UserPage } from './../user/UserPage';
import { UsersPage } from './../users/UsersPage';
import { AppDashboardPage } from './AppDashboardPage';
import { AppSettingsPage } from './AppSettingsPage';

const DesignItem = ({ match, path }: { match: match<{}>, path: string }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const doToggle = React.useCallback(() =>  {
        setIsOpen(prev => !prev);
    }, []);

    const doClose = React.useCallback(() =>  {
        setIsOpen(false);
    }, []);

    const isActive =
        path.startsWith(`${match.url}/media`) ||
        path.startsWith(`${match.url}/templates`) ||
        path.startsWith(`${match.url}/emails`);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={doToggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='create' /> <span>{texts.common.design}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink activeClassName='active' className='dropdown-item' to={`${match.url}/media`} onClick={doClose}>
                    <Icon type='photo_size_select_actual' /> <span>{texts.media.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={`${match.url}/templates`} onClick={doClose}>
                    <Icon type='file_copy' /> <span>{texts.templates.header}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={`${match.url}/emails`} onClick={doClose}>
                    <Icon type='mail_outline' /> <span>{texts.emails.header}</span>
                </NavLink>
            </DropdownMenu>
        </Dropdown>
    );
};

const MoreItem = ({ match, path }: { match: match<{}>, path: string }) => {
    const [isOpen, setIsOpen] = React.useState(false);

    const doToggle = React.useCallback(() =>  {
        setIsOpen(prev => !prev);
    }, []);

    const doClose = React.useCallback(() =>  {
        setIsOpen(false);
    }, []);

    const isActive =
        path.startsWith(`${match.url}/settings`) ||
        path.startsWith(`${match.url}/log`);

    return (
        <Dropdown nav direction='right' isOpen={isOpen} toggle={doToggle} active={isActive}>
            <DropdownToggle nav caret>
                <Icon type='settings' /> <span>{texts.common.more}</span>
            </DropdownToggle>
            <DropdownMenu>
                <NavLink activeClassName='active' className='dropdown-item' to={`${match.url}/settings`}onClick={doClose}>
                    <Icon type='settings' /> <span>{texts.common.settings}</span>
                </NavLink>
                <NavLink activeClassName='active' className='dropdown-item' to={`${match.url}/log`} onClick={doClose}>
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
    const app = useApps(getApp);
    const appId = match.params['appId'];
    const loading = useApps(x => x.apps.isLoading);

    React.useEffect(() => {
        dispatch(selectApp(appId));
    }, [appId]);

    const doPublish = React.useCallback(() => {
        dispatch(openPublishDialog());
    }, []);

    if (loading) {
        return null;
    }

    if (!app) {
        return <>{texts.apps.notFound}</>;
    }

    return (
        <>
            <Nav vertical className='sidebar'>
                <NavItem>
                    <NavLink activeClassName='active' className='nav-link' to={`${match.url}`} exact>
                        <Icon type='dashboard' /> <span>{texts.common.dashboard}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink activeClassName='active' className='nav-link' to={`${match.url}/users`}>
                        <Icon type='person' /> <span>{texts.users.header}</span>
                    </NavLink>
                </NavItem>

                <NavItem>
                    <NavLink activeClassName='active' className='nav-link' to={`${match.url}/events`}>
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
                <Route path={`${match.url}/users/:userId`}>
                    <UserPage />
                </Route>

                <Route path={`${match.url}/users`} exact>
                    <UsersPage />
                </Route>

                <Route path={`${match.url}/events`} exact>
                    <EventsPage />
                </Route>

                <Route path={`${match.url}/templates`} exact>
                    <TemplatesPage />
                </Route>

                <Route path={`${match.url}/media`} exact>
                    <MediaPage />
                </Route>

                <Route path={`${match.url}/emails`} exact>
                    <EmailsPage />
                </Route>

                <Route path={`${match.url}/log`} exact>
                    <LogPage />
                </Route>

                <Route path={`${match.url}/settings`} exact>
                    <AppSettingsPage />
                </Route>

                <Route render={_ =>
                    <AppDashboardPage />
                } />
            </Switch>

            <PublishDialog />
        </>
    );
};
