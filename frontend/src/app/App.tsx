/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { InternalPage } from '@app/pages/InternalPage';
import { RouteWhenPrivate } from '@app/shared/components';
import { useLogin } from '@app/state';
import * as React from 'react';
import { Redirect, Route, Switch } from 'react-router';
import { ToastContainer } from 'react-toastify';
import ReactTooltip from 'react-tooltip';
import { ErrorBoundary } from '@app/framework';
import { AuthenticationPage } from './pages/authentication/AuthenticationPage';

export const App = () => {
    const isAuthenticated = useLogin(x => !!x.user);

    return (
        <ErrorBoundary>
            <Switch>
                <RouteWhenPrivate path='/app' isAuthenticated={isAuthenticated}
                    component={InternalPage} />

                <Route path='/authentication'>
                    <AuthenticationPage />
                </Route>

                <Route render={() =>
                    <Redirect to='/app' />
                } />
            </Switch>

            <ReactTooltip place='top' effect='solid' />

            <ToastContainer position='bottom-right' />
        </ErrorBoundary>
    );
};
