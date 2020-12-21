/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { loginDoneAsync, logoutDoneAsync } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Switch, useRouteMatch } from 'react-router';

export const LoginCallbackPage = () => {
    const dispatch = useDispatch();

    React.useEffect(() => {
        dispatch(loginDoneAsync());
    }, []);

    return <></>;
};

export const LogoutCallbackPage = () => {
    const dispatch = useDispatch();

    React.useEffect(() => {
        dispatch(logoutDoneAsync());
    }, []);

    return <></>;
};

export const AuthenticationPage = () => {
    const match = useRouteMatch();

    return (
        <Switch>
            <Route path={`${match.path}/login-callback`} exact>
                <LoginCallbackPage />
            </Route>
            <Route path={`${match.path}/logout-callback`} exact>
                <LogoutCallbackPage />
            </Route>
        </Switch>
    );
};
