/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, Routes } from 'react-router';
import { loginDone, logoutDone } from '@app/state';

export const LoginCallbackPage = () => {
    const dispatch = useDispatch<any>();

    React.useEffect(() => {
        dispatch(loginDone());
    }, [dispatch]);

    return <></>;
};

export const LogoutCallbackPage = () => {
    const dispatch = useDispatch<any>();

    React.useEffect(() => {
        dispatch(logoutDone() as any);
    }, [dispatch]);

    return <></>;
};

export const AuthenticationPage = () => {
    return (
        <Routes>
            <Route path='login-callback'
                element={<LoginCallbackPage />} />

            <Route path='logout-callback'
                element={<LogoutCallbackPage />} />
        </Routes>
    );
};
