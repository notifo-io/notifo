/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export const Gist = ({ id }: { id: string }) => {
    const [iframe, setIFrame] = React.useState<HTMLIFrameElement>();

    const elementId = `${id}`;

    React.useEffect(() => {
        if (iframe && id) {
            const gistLink = `https://gist.github.com/${id}.js`;
            const gistScript = `<script type="text/javascript" src="${gistLink}"></script>`;

            const resizeScript = `onload="parent.document.getElementById('${elementId}').style.height=document.body.offsetHeight + 'px'"`;
            const iframeStyles = '<style>body { margin: 0; overflow: hidden; }</style>';
            const iframeHtml = `<html><head><base target="_parent">${iframeStyles}</head><body ${resizeScript}>${gistScript}</body></html>`;

            const document = iframe.contentDocument;

            document.open();
            document.writeln(iframeHtml);
            document.close();
        }
    }, [iframe, id]);

    return (
        <iframe id={elementId} ref={setIFrame} width='100%' frameBorder={0} />
    );
};
