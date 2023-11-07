/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { loginStart } from '@app/state';

export interface RouteWhenPrivateProps extends React.PropsWithChildren {
    // The current auth state.
    isAuthenticated: boolean;
}

export const RouteWhenPrivate = (props: RouteWhenPrivateProps) => {
    const { children, isAuthenticated } = props;

    const dispatch = useDispatch<any>();

    React.useEffect(() => {
        if (!isAuthenticated) {
            dispatch(loginStart());
        }
    }, [dispatch, isAuthenticated]);

    if (!isAuthenticated) {
        return null;
    }

    return children;
};
