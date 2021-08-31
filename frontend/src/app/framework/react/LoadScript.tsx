/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export interface ScriptProps {
    // The script url.
    url: string;

    // Optional callback when created.
    onCreate?: () => void;

    // Optional callback when destroyed.
    onDestroy?: () => void;

    // Optional callback when loaded.
    onLoad?: () => void;
}

export const Script = (props: ScriptProps) => {
    const { onCreate, onDestroy, onLoad, url } = props;

    React.useEffect(() => {
        const script = document.createElement('script');

        onCreate && onCreate();

        script.src = url;

        if (!script.hasAttribute('async')) {
            script.async = true;
        }

        script.onload = () => {
            onLoad && onLoad();
        };

        script.onerror = () => {
            onDestroy && onDestroy();
        };

        document.body.appendChild(script);
    }, [onCreate, onDestroy, onLoad, url]);

    return <></>;
};
