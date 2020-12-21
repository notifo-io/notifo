/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export const Title = ({ text }: { text: string }) => {
    React.useEffect(() => {
        document.title = text;
    }, [text]);

    return <></>;
};
