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
        const notifo = window['notifo'] || (window['notifo'] = []);

        notifo.push(['init', {
            userToken: token,
        }]);

        notifo.push(['subscribe']);

        notifo.push(['show-notifications', 'notifo-button', {
            position: 'bottom-right',

            // The profile is manage dby the normal settings
            hideProfile: true,
        }]);
    }, [ token]);

    return (
        <div id='notifo-button'></div>
    );
};