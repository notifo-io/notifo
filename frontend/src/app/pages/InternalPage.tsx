/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useRouteMatch } from 'react-router';
import { ErrorBoundary } from '@app/framework';
import { loadApps, loadLanguages, loadMjmlSchema, loadProfile, loadTimezones } from '@app/state';
import { TopNav } from './TopNav';
import { AppPage } from './app/AppPage';
import { AppsPage } from './apps/AppsPage';
import { SystemUsersPage } from './system-users/SystemUsersPage';

export const InternalPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();

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
                <Switch>
                    <Route path={`${match.path}/system-users`}>
                        <SystemUsersPage />
                    </Route>

                    <Route path={`${match.path}/:appId`}>
                        <AppPage />
                    </Route>

                    <Route path={match.path} exact>
                        <AppsPage />
                    </Route>
                </Switch>
            </ErrorBoundary>
        </>
    );
};
