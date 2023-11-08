/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Routes } from 'react-router';
import { ErrorBoundary } from '@app/framework';
import { loadApps, loadLanguages, loadMjmlSchema, loadProfile, loadTimezones } from '@app/state';
import { TopNav } from './TopNav';
import { AppPage } from './app/AppPage';
import { AppsPage } from './apps/AppsPage';
import { SystemUsersPage } from './system-users/SystemUsersPage';

export const InternalPage = () => {
    const dispatch = useDispatch<any>();

    React.useEffect(() => {
        dispatch(loadProfile());
        dispatch(loadApps());
        dispatch(loadLanguages());
        dispatch(loadTimezones());
        dispatch(loadMjmlSchema());
    }, [dispatch]);

    return (
        <>
            <TopNav />

            <ErrorBoundary>
                <Routes>
                    <Route path='system-users'
                        element={<SystemUsersPage />} />

                    <Route path=':appId/*'
                        element={<AppPage />} />

                    <Route path='*'
                        element={<AppsPage />} />
                </Routes>
            </ErrorBoundary>
        </>
    );
};
