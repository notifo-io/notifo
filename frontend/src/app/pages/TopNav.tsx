/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import { NavLink } from 'react-router-dom';
import { Dropdown, DropdownItem, DropdownMenu, DropdownToggle, Nav, Navbar } from 'reactstrap';
import { AppsDropdown, Logo } from '@app/shared/components';
import { loadApps, loadLanguages, loadTimezones, logoutStart, useLogin } from '@app/state';
import { texts } from '@app/texts';

export const TopNav = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const user = useLogin(x => x.user)!;
    const [isOpen, setOpen] = React.useState(false);

    const doOpen = React.useCallback(() => {
        setOpen(true);
    }, []);

    const doClose = React.useCallback(() => {
        setOpen(false);
    }, []);

    const doToggle = React.useCallback(() => {
        setOpen(!isOpen);

        return false;
    }, [isOpen]);

    React.useEffect(() => {
        dispatch(loadApps());
        dispatch(loadLanguages());
        dispatch(loadTimezones());
    }, [dispatch]);

    const doLogout = React.useCallback(() => {
        dispatch(logoutStart());
    }, [dispatch]);

    return (
        <Navbar dark fixed='top' color='primary'>
            <NavLink to={match.url} className='navbar-brand'>
                <Logo />
            </NavLink>

            <Nav navbar>
                <AppsDropdown />
            </Nav>

            <Nav navbar className='ml-auto'>
                <Dropdown nav inNavbar isOpen={isOpen} toggle={doToggle}>
                    <DropdownToggle nav caret onClick={doOpen}>
                        {texts.common.profile}
                    </DropdownToggle>
                    <DropdownMenu right>
                        <DropdownItem onClick={doClose} href='/account/profile' target='_blank'>
                            <div>{texts.common.welcome},</div>

                            <strong>{user.name}</strong>
                        </DropdownItem>

                        <DropdownItem divider />

                        <DropdownItem onClick={doClose} href='/account/profile' target='_blank'>
                            {texts.common.profileSettings}
                        </DropdownItem>

                        {user.roles.find(x => x?.toUpperCase() === 'ADMIN') &&
                            <NavLink onClick={doClose} to={`${match.path}/system-users`} className='dropdown-item'>
                                {texts.systemUsers.header}
                            </NavLink>
                        }

                        <DropdownItem divider />

                        <DropdownItem onClick={doLogout}>
                            {texts.common.logout}
                        </DropdownItem>
                    </DropdownMenu>
                </Dropdown>
            </Nav>
        </Navbar>
    );
};
