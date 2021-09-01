/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';

export interface ClickOutsideProps extends React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement> {
    // When clicked outside.
    onClickOutside: () => any;

    // Indicates whether the outside click handler is active.
    disabled?: boolean | null;

    // The children.
    children: React.ReactNode;
}

export const ClickOutside = React.memo((props: ClickOutsideProps) => {
    const { children, disabled, onClickOutside, ...other } = props;

    const container = React.useRef<HTMLDivElement>();

    const setContainer = React.useCallback((div: HTMLDivElement) => {
        container.current = div;
    }, []);

    React.useEffect(() => {
        const onClick = (event: MouseEvent) => {
            if (container.current && !container.current.contains(event.target as any)) {
                onClickOutside();
            }
        };

        if (!disabled) {
            document.addEventListener('click', onClick, true);

            return () => {
                document.removeEventListener('click', onClick, true);
            };
        }

        return undefined;
    }, [disabled, onClickOutside]);

    return (
        <div {...other} ref={setContainer}>
            {children}
        </div>
    );
});
