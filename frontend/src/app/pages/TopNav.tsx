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
import { Dropdown, DropdownItem, DropdownMenu, DropdownToggle, Nav, Navbar, NavItem } from 'reactstrap';
import { useBoolean, useEventCallback } from '@app/framework';
import { AppsDropdown, Integrated, Logo } from '@app/shared/components';
import { loadApps, loadLanguages, loadTimezones, logoutStart, useLogin } from '@app/state';
import { texts } from '@app/texts';

export const TopNav = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const user = useLogin(x => x.user)!;
    const userProfile = useLogin(x => x.profile);
    const [isOpen, setIsOpen] = useBoolean();

    React.useEffect(() => {
        dispatch(loadApps());
        dispatch(loadLanguages());
        dispatch(loadTimezones());
    }, [dispatch]);

    const doLogout = useEventCallback(() => {
        dispatch(logoutStart());
    });

    return (
        <Navbar dark fixed='top' color='primary'>
            <NavLink to={match.url} className='navbar-brand'>
                <Logo />
            </NavLink>

            <Nav navbar>
                <AppsDropdown />
            </Nav>

            <Nav navbar className='ml-auto'>
                {userProfile?.token &&
                    <NavItem>
                        <Integrated token={userProfile.token} />
                    </NavItem>
                }

                <Dropdown nav inNavbar isOpen={isOpen} toggle={setIsOpen.toggle}>
                    <DropdownToggle nav caret>
                        {texts.common.profile}
                    </DropdownToggle>
                    <DropdownMenu right>
                        <DropdownItem onClick={setIsOpen.off} href='/account/profile' target='_blank'>
                            <div>{texts.common.welcome},</div>

                            <strong>{user.name}</strong>
                        </DropdownItem>

                        <DropdownItem divider />

                        <DropdownItem onClick={setIsOpen.off} href='/account/profile' target='_blank'>
                            {texts.common.profileSettings}
                        </DropdownItem>

                        {user.roles.find(x => x?.toUpperCase() === 'ADMIN') &&
                            <NavLink onClick={setIsOpen.off} to={`${match.path}/system-users`} className='dropdown-item'>
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
