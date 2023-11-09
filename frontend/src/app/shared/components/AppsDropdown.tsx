/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Link } from 'react-router-dom';
import { Dropdown, DropdownMenu, DropdownToggle } from 'reactstrap';
import { useBooleanObj } from '@app/framework';
import { useApp, useApps } from '@app/state';

export const AppsDropdown = () => {
    const app = useApp();
    const apps = useApps(x => x.apps);
    const dropdown = useBooleanObj();

    if (!app || !apps.items) {
        return null;
    }

    return (
        <Dropdown isOpen={dropdown.value} className='apps-dropdown' nav inNavbar toggle={dropdown.toggle}>
            <DropdownToggle nav caret>
                <span className='apps-dropdown-name'>{app.name}</span>
            </DropdownToggle>
            <DropdownMenu>
                {apps.items.map(app =>
                    <Link key={app.id} to={app.id} className='dropdown-item' onClick={dropdown.off}>
                        {app.name}
                    </Link>,
                )}
            </DropdownMenu>
        </Dropdown>
    );
};
