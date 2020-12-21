/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { loadAppsAsync, loadLanguagesAsync, loadTimezonesAsync, logoutStartAsync, useLogin } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useRouteMatch } from 'react-router';
import { NavLink } from 'react-router-dom';
import { DropdownItem, DropdownMenu, DropdownToggle, Nav, Navbar, UncontrolledDropdown } from 'reactstrap';
import { AppPage } from './app/AppPage';
import { AppsPage } from './apps/AppsPage';

export const InternalPage = () => {
    const match = useRouteMatch();
    const dispatch = useDispatch();
    const user = useLogin(x => x.user);

    React.useEffect(() => {
        dispatch(loadAppsAsync());
        dispatch(loadLanguagesAsync());
        dispatch(loadTimezonesAsync());
    }, []);

    const doLogout = React.useCallback(() => {
        dispatch(logoutStartAsync());
    }, []);

    return (
        <>
            <Navbar dark fixed='top' color='primary'>
                <NavLink to={match.url} className='navbar-brand'>
                    <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 403.2 403.2'>
                        <g fill='#fff'>
                            <g transform='translate(63.6 -6.05)'>
                                <path d='M60.2 149.7c-16.8 0-32-5.9-44.3-15.5 0 .5-.1 1.1-.1 1.6v192h53.7v-179c-3.1.4-6.1.9-9.3.9zM138.1 13.6c-12.3 0-24.1 1.9-35.3 5.2 15.7 11.5 26.3 29 28.9 49.1 2.1-.2 4.2-.6 6.3-.6 37.7 0 68.5 30.8 68.5 68.5v192h53.7v-192c.1-67.4-54.7-122.2-122.1-122.2z' />

                                <circle r='43.4' cy='77.2' cx='60.2' fill='#ef3f5d' />
                            </g>
                            <path d='M201.65 395.65c27.1 0 49.2-22.1 49.2-49.2h-98.5c0 27 22.2 49.2 49.3 49.2z' />
                        </g>
                    </svg>
                </NavLink>

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

                            <DropdownItem onClick={doLogout}>
                                {texts.common.logout}
                            </DropdownItem>
                        </DropdownMenu>
                    </UncontrolledDropdown>
                </Nav>
            </Navbar>

            <main>
                <Switch>
                    <Route path={`${match.path}/:appId`}>
                        <AppPage />
                    </Route>

                    <Route path={match.path} exact>
                        <AppsPage />
                    </Route>
                </Switch>
            </main>
        </>
    );
};
