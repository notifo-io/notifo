/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { loginStart } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Route, RouteProps } from 'react-router';

export interface RouteWhenPrivateProps {
    // The current auth state.
    isAuthenticated: boolean;

    // The component to render when the user is not authenticated.
    component: React.ComponentType<any>;
}

export const RouteWhenPrivate = (props: RouteWhenPrivateProps & RouteProps) => {
    const { component: Component, isAuthenticated, ...routeProps } = props;

    const dispatch = useDispatch();

    React.useEffect(() => {
        if (!isAuthenticated) {
            dispatch(loginStart());
        }
    }, [dispatch, isAuthenticated]);

    if (!isAuthenticated) {
        return null;
    }

    return (
        <Route {...routeProps} render={p => <Component {...p} />} />
    );
};
