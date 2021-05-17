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

    React.useEffect(() => {
        if (!disabled) {
            Mousetrap.bind(keys, (event) => {
                onPressed();

                event.preventDefault();
                event.stopPropagation();
                event.stopImmediatePropagation();

                return false;
            });

            return () => {
                Mousetrap.unbind(keys);
            };
        }

        return undefined;
    }, [disabled, keys, onPressed]);

    return <></>;
};
