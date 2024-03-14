/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Navigate, Route, Routes } from 'react-router';
import { ToastContainer } from 'react-toastify';
import { Tooltip as ReactTooltip } from 'react-tooltip';
import { ErrorBoundary } from '@app/framework';
import { RouteWhenPrivate } from '@app/shared/components';
import { useLogin } from '@app/state';
import { InternalPage } from './pages/InternalPage';
import { AuthenticationPage } from './pages/authentication/AuthenticationPage';
import { DemoPage } from './pages/user/DemoPage';

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

                <Route path='/authentication/*'
                    element={<AuthenticationPage />} />

                <Route path='/demo/:userId'
                    element={<DemoPage />} />

                <Route index
                    element={<Navigate to='/app' />} />

                <Route path='*'
                    element={<Navigate to='/app' />} />
            </Routes>

            <ReactTooltip place='top' id='default-tooltip' />

            <ToastContainer position='bottom-right' />
        </ErrorBoundary>
    );
};
