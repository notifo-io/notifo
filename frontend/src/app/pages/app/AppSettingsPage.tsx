/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { loadDetails, useApp, useApps } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { AppSettings } from './AppSettings';
import { Contributors } from './Contributors';

export const AppSettingsPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appDetails = useApps(x => x.app);

    React.useEffect(() => {
        dispatch(loadDetails({ appId }));
    }, [appId]);

    if (!appDetails) {
        return null;
    }

    return (
        <div className='settings'>
            <Contributors appDetails={appDetails} />

            <AppSettings appDetails={appDetails} />
        </div>
    );
};
