/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import markerSDK, { MarkerSdk } from '@marker.io/browser';
import * as React from 'react';
import { NavLink } from 'reactstrap';
import { texts } from '@app/texts';
import { useEventCallback } from './hooks';

export interface MarkerProps {
    projectId?: string;
}

export const Marker = (props: MarkerProps) => {
    const { projectId } = props;
    const widgetRef = React.useRef<MarkerSdk>();

    React.useEffect(() => {            
        if (projectId) {
            const setup = async (project: string) => {
                const widget = await markerSDK.loadWidget({ project });

                widget.hide();
                widgetRef.current = widget;
            };

            setup(projectId);
        }

        return () => {
            widgetRef.current?.unload();
            widgetRef.current = undefined;
        };
    }, [projectId]);

    const doCapture = useEventCallback(() => {
        widgetRef.current?.capture('fullscreen');
    });

    return (
        <NavLink className='cursor-pointer' onClick={doCapture}>
            {texts.common.feedback}
        </NavLink>
    );
};