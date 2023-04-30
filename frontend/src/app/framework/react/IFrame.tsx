/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useEventCallback } from './hooks';

export interface IFrameProps {
    html?: string | null;

    // The scrolling
    scrolling?: 'auto' | 'yes' | 'no';

    // The style.
    style?: React.CSSProperties;
}

interface State {
    iframeActive?: HTMLIFrameElement;
    iframeInactive?: HTMLIFrameElement;
    html?: string | null;
}

export const IFrame = (props: IFrameProps) => {
    const { html, scrolling, style } = props;

    const currentState = React.useRef<State>({});

    const doInit1 = useEventCallback((iframe: HTMLIFrameElement) => {
        if (!iframe) {
            return;
        }

        currentState.current.iframeActive = iframe;
        startLoading();
    });

    const doInit2 = useEventCallback((iframe: HTMLIFrameElement) => {
        if (!iframe) {
            return;
        }
        
        currentState.current.iframeInactive = iframe;
        currentState.current.iframeInactive.style.display = 'none';
    });

    const commitLoading = useEventCallback((event: React.SyntheticEvent<HTMLIFrameElement>) => {
        const state = currentState.current;

        if (event.currentTarget === state.iframeInactive) {
            const active = state.iframeActive;

            state.iframeActive = event.currentTarget;
            state.iframeInactive = active;

            if (state.iframeActive) {
                state.iframeActive.style.display = 'block';
            }

            if (state.iframeInactive) {
                state.iframeInactive.style.display = 'none';
            }
        }
    });

    const startLoading = useEventCallback(() => {
        const state = currentState.current;

        if (state.html && state.iframeInactive) {
            const document = state.iframeInactive.contentDocument;

            if (document) {
                document.open();
                document.write(state.html || '');
                document.close();
            }
        }
    });

    React.useEffect(() => {
        currentState.current.html = html;

        startLoading();
    }, [html, startLoading]);

    return (
        <>
            <iframe style={style} scrolling={scrolling} onLoad={commitLoading} ref={doInit1} />
            <iframe style={style} scrolling={scrolling} onLoad={commitLoading} ref={doInit2} />
        </>
    );
};
