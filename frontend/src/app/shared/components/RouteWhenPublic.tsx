/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Redirect, Route, RouteProps } from 'react-router';
import * as React from 'react';

export interface RouteWhenPublicProps {
    // The current auth state.
    isAuthenticated: boolean;

    // The component to render when the user is not authenticated.
    component: React.Component;
}

export const RouteWhenPublic = (props: RouteWhenPublicProps & RouteProps) => {
    const { component: Component, isAuthenticated, ...routeProps } = props;

    return (
        <Route {...routeProps}
            render={p =>
                (!isAuthenticated ? (
                    <Component {...p} />
                ) : (
                    <Redirect to='/app' from={p.location.pathname} />
                ))
            } />
    );
};
