/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable no-console */

import * as React from 'react';

type Fn<ARGS extends any[], R> = (...args: ARGS) => R;

export const useEventCallback = <A extends any[], R>(fn: Fn<A, R>): Fn<A, R> => {
    let ref = React.useRef<Fn<A, R>>(fn);

    React.useLayoutEffect(() => {
        ref.current = fn;
    });

    return React.useMemo(() => (...args: A): R => {
        return ref.current(...args);
    }, []);
};

type BooleanSetter = {
    toggle: () => void;
    off: () => void;
    on: () => void;
    setValue: (value: boolean) => void;
};

export function useBoolean(initialValue = false): [boolean, BooleanSetter] {
    const [value, setValue] = React.useState(initialValue);

    const setter = React.useMemo(() => {
        return {
            toggle: () => { 
                setValue(x => !x);
                return false;
            },
            on: () => { 
                setValue(true);
                return false;
            },
            off: () => { 
                setValue(false);
                return false;
            },
            setValue: (value: boolean) => {
                setValue(value);
                return false;
            },
        };
    }, []);

    return [value, setter];
}

export function useBooleanObj(initialValue = false): BooleanSetter & { value: boolean } {
    const [, setValue] = React.useState(initialValue);
    const valueRef = React.useRef(initialValue);

    const setter = React.useMemo(() => {
        const setValueCore = (value: boolean) => {
            valueRef.current = value;
            setValue(value);
        };

        const result = {
            toggle: () => { 
                setValueCore(!valueRef.current);
                return false;
            },
            on: () => { 
                setValueCore(true);
                return false;
            },
            off: () => { 
                setValueCore(false);
                return false;
            },
            setValue: (value: boolean) => {
                setValueCore(value);
                return false;
            },
        };

        Object.defineProperty(result, 'value', {
            get: () => {
                return valueRef.current;
            },
        });

        return result as any;
    }, []);

    return setter;
}

export function useSavedState<T>(initial: T, key: string): [T, (newValue: T) => void] {
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

    const [value, setValue] = React.useState(factory);

    const valueSetter = useEventCallback((newValue: T) => {
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
    });

    return [value, valueSetter];
}

export function usePrevious <T>(value: T) {
    const ref = React.useRef<T>();

    React.useEffect(() => {
        ref.current = value;
    });

    return ref.current;
}