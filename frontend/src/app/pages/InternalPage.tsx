/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useRouteMatch } from 'react-router';
import { NavLink } from 'react-router-dom';
import { DropdownItem, DropdownMenu, DropdownToggle, Nav, Navbar, UncontrolledDropdown } from 'reactstrap';
import { ErrorBoundary } from '@app/framework';
import { AppsDropdown, Logo } from '@app/shared/components';
import { loadApps, loadLanguages, loadTimezones, logoutStart, useLogin } from '@app/state';
import { texts } from '@app/texts';
import { AppPage } from './app/AppPage';
import { AppsPage } from './apps/AppsPage';
import { SystemUsersPage } from './system-users/SystemUsersPage';

export const InternalPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const user = useLogin(x => x.user)!;

    React.useEffect(() => {
        dispatch(loadApps());
        dispatch(loadLanguages());
        dispatch(loadTimezones());
    }, [dispatch]);

    const doLogout = React.useCallback(() => {
        dispatch(logoutStart());
    }, [dispatch]);

    return (
        <>
            <Navbar dark fixed='top' color='primary'>
                <NavLink to={match.url} className='navbar-brand'>
                    <Logo />
                </NavLink>

                <Nav navbar>
                    <AppsDropdown />
                </Nav>

                <Nav navbar className='ml-auto'>
                    <UncontrolledDropdown nav inNavbar>
                        <DropdownToggle nav caret>
                            {texts.common.profile}
                        </DropdownToggle>
                        <DropdownMenu right>
                            <DropdownItem>
                                <div>{texts.common.welcome},</div>

                                <strong>{user.name}</strong>
                            </DropdownItem>

                            <DropdownItem divider />

                            <DropdownItem href='/account/profile' target='_blank'>
                                {texts.common.profileSettings}
                            </DropdownItem>

                            {user.role?.toLowerCase() === 'admin' &&
                                <NavLink to={`${match.path}/system-users`} className='dropdown-item'>
                                    {texts.systemUsers.header}
                                </NavLink>
                            }

                            <DropdownItem divider />

                            <DropdownItem onClick={doLogout}>
                                {texts.common.logout}
                            </DropdownItem>
                        </DropdownMenu>
                    </UncontrolledDropdown>
                </Nav>
            </Navbar>

            <ErrorBoundary>
                <Switch>
                    <Route path={`${match.path}/system-users`}>
                        <SystemUsersPage />
                    </Route>

                    <Route path={`${match.path}/:appId`}>
                        <AppPage />
                    </Route>

                    <Route path={match.path} exact>
                        <AppsPage />
                    </Route>
                </Switch>
            </ErrorBoundary>
        </>
    );
};
