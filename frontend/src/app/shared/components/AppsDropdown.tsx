/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { useApp, useApps } from '@app/state';
import * as React from 'react';
import { Link, useRouteMatch } from 'react-router-dom';
import { Dropdown, DropdownMenu, DropdownToggle } from 'reactstrap';

export const AppsDropdown = () => {
    const app = useApp();
    const apps = useApps(x => x.apps);
    const match = useRouteMatch();

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

    if (!app || !apps.items) {
        return null;
    }

    return (
        <Dropdown isOpen={isOpen} className='apps-dropdown' nav inNavbar toggle={doToggle}>
            <DropdownToggle nav caret onClick={doOpen}>
                <span className='apps-dropdown-name'>{app.name}</span>
            </DropdownToggle>
            <DropdownMenu>
                {apps.items.map(app =>
                    <Link key={app.id} to={`${match.path}/${app.id}`} className='dropdown-item' onClick={doClose}>
                        {app.name}
                    </Link>,
                )}
            </DropdownMenu>
        </Dropdown>
    );
};
