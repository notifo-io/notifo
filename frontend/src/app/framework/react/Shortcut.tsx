/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as Mousetrap from 'mousetrap';
import * as React from 'react';

export interface ShortcutProps {
    // Disable the shortcut
    disabled?: boolean;

    // The key binding.
    keys: string;

    // Triggered when the keys are pressed.
    onPressed: () => any;
}

export const Shortcut = (props: ShortcutProps) => {
    const { disabled, keys, onPressed } = props;

    const currentDisabled = React.useRef(disabled);
    const currentOnPressed = React.useRef(onPressed);

    currentDisabled.current = disabled;
    currentOnPressed.current = onPressed;

    React.useEffect(() => {
        Mousetrap.bind(keys, (event) => {
            if (!currentDisabled.current) {
                currentOnPressed.current();
            }

            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            return false;
        });

        return () => {
            Mousetrap.unbind(keys);
        };
    }, [keys]);

    return <></>;
};
