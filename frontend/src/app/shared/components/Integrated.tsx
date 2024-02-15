/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export interface IntegratedProps {
    // The notifo token.
    token: string;
}

export const Integrated = (props: IntegratedProps) => {
    const { token } = props;

    React.useEffect(() => {
        if (import.meta.env.PROD) {
            const script = document.createElement('script');
            script.src = '/notifo-sdk.js';
            script.async = true;
    
            document.body.appendChild(script);
        }
    }, []);

    React.useEffect(() => {
        const notifo = (window as any)['notifo'] || ((window as any)['notifo'] = []);

        notifo.push(['init', {
            userToken: token,
            userId: null,
            apiUrl: '/',
        }]);

        notifo.push(['subscribe']);

        notifo.push(['show-notifications', 'notifo-button', {
            position: 'bottom-right',

            // The profile is manage dby the normal settings
            hideProfile: true,
        }]);
    }, [token]);

    return (
        <div id='notifo-button'></div>
    );
};