/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export interface IFrameProps {
    html?: string | null;

    // The scrolling
    scrolling?: 'auto' | 'yes' | 'no';

    // The style.
    style?: React.CSSProperties;
}

export const IFrame = (props: IFrameProps) => {
    const { html, scrolling, style } = props;

    const [iframe, setIframe] = React.useState<HTMLIFrameElement | null>(null);

    React.useEffect(() => {
        if (iframe && html) {
            const document = iframe.contentDocument;

            if (document) {
                document.open();
                document.write(html || '');
                document.close();
            }
        }
    }, [iframe, html]);

    return (
        <iframe style={style} scrolling={scrolling} ref={setIframe} />
    );
};
