/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable no-console */

import * as React from 'react';

export function useDialog() {
    const [isOpen, setIsOpen] = React.useState(false);

    const open = React.useCallback(() => {
        setIsOpen(true);
    }, []);

    const close = React.useCallback(() => {
        setIsOpen(false);
    }, []);

    return { isOpen, open, close };
}

export function useSavedState<T>(initial: T, key: string): [T, (newValue: T) => void] {
    if (!window.localStorage) {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        return React.useState(initial);
    }

    const factory = () => {
        try {
            const stored = window.localStorage.getItem(key);

            if (stored) {
                return JSON.parse(stored);
            }
        } catch {
            console.debug(`NOTIFO SDK: Failed to read '${key}' from local store`);
        }

        return initial;
    };

    // eslint-disable-next-line react-hooks/rules-of-hooks
    const [value, setValue] = React.useState(factory);

    // eslint-disable-next-line react-hooks/rules-of-hooks
    const valueSetter = React.useCallback((newValue: T) => {
        try {
            if (newValue) {
                const serialized = JSON.stringify(newValue);

                window.localStorage.setItem(key, serialized);
            } else {
                window.localStorage.removeItem(key);
            }
        } catch {
            console.debug(`NOTIFO SDK: Failed to write '${key}' to local store`);
        }

        setValue(newValue);
    }, [key]);

    return [value, valueSetter];
}

export function usePrevious <T>(value: T) {
    const ref = React.useRef<T>();

    React.useEffect(() => {
        ref.current = value;
    });

    return ref.current;
}

export function useStateWithRef<T>(initial: T): [T, (value: T) => void, { current: T }] {
    const [state, setState] = React.useState<T>(initial);
    const snapshot = React.useRef(initial);

    const update = React.useCallback((value: T) => {
        snapshot.current = value;
        setState(value);
    }, []);

    return [state, update, snapshot];
}
