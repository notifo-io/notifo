/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useEventCallback } from './hooks';

export interface ClickOutsideProps extends React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement>, React.PropsWithChildren {
    // When clicked outside.
    onClickOutside: (event: MouseEvent) => any;

    // Indicates whether the outside click handler is active.
    isActive: boolean;
}

export const ClickOutside = React.memo((props: ClickOutsideProps) => {
    const { children, isActive, onClickOutside, ...other } = props;

    const container = React.useRef<HTMLDivElement>();

    const initContainer = useEventCallback((div: HTMLDivElement) => {
        container.current = div;
    });

    React.useEffect(() => {
        const onClick = (event: MouseEvent) => {
            if (container.current && !container.current.contains(event.target as any)) {
                onClickOutside(event);
            }
        };

        if (isActive) {
            document.addEventListener('click', onClick, true);

            return () => {
                document.removeEventListener('click', onClick, true);
            };
        }

        return undefined;
    }, [isActive, onClickOutside]);

    return (
        <div {...other} ref={initContainer}>
            {children}
        </div>
    );
});
