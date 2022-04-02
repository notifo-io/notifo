/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { MutableRef, StateUpdater, useCallback, useMemo, useRef, useState } from 'preact/hooks';
import { isFunction } from '@sdk/shared';

export function useToggle(initialValue = false) {
    const [isOpen, setIsOpen] = useState(initialValue);

    const show = useCallback(() => {
        setIsOpen(true);
    }, []);

    const hide = useCallback(() => {
        setIsOpen(false);
    }, []);

    return { isOpen, hide, show };
}

export function useMutable<T>(initialValue: T) {
    const ref = useRef<T>(initialValue);

    // Just used to force an update.
    const [, setUpdater] = useState(0);

    return useMemo(() => new MutableUpdater<T>(ref, setUpdater), []);
}

class MutableUpdater<T> {
    public get current() {
        return this.ref.current;
    }

    constructor(
        private readonly ref: MutableRef<T>,
        private readonly updater: StateUpdater<number>,
    ) {
    }

    public refresh() {
        this.updater(x => x + 1);
    }

    public set(value: T | ((value: T) => void)) {
        if (isFunction(value)) {
            value(this.ref.current);
        } else {
            this.ref.current = value;
        }

        this.refresh();
    }
}