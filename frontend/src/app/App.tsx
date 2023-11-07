/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Navigate, Route, Routes } from 'react-router';
import { ToastContainer } from 'react-toastify';
import { Tooltip as ReactTooltip } from 'react-tooltip';
import { ErrorBoundary } from '@app/framework';
import { RouteWhenPrivate } from '@app/shared/components';
import { useLogin } from '@app/state';
import { InternalPage } from './pages/InternalPage';
import { AuthenticationPage } from './pages/authentication/AuthenticationPage';

export const App = () => {
    const isAuthenticated = useLogin(x => !!x.user);

    return (
        <ErrorBoundary>
            <Routes>
                <Route path='/app/*' element={
                    <RouteWhenPrivate isAuthenticated={isAuthenticated}>
                        <InternalPage />
                    </RouteWhenPrivate>
                } />

                <Route path='/authentication'
                    element={<AuthenticationPage />} />

                <Route index
                    element={<Navigate to='/app' />} />
            </Routes>

            <ReactTooltip place='top' />

            <ToastContainer position='bottom-right' />
        </ErrorBoundary>
    );
};
