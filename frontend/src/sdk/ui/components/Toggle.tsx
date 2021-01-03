/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { isBoolean, isUndefined } from '@sdk/shared';
import { useEffect, useState } from 'preact/hooks';

export interface ToggleProps {
    // The current value.
    value?: boolean;

    // Set to allow three states.
    indeterminate?: boolean;

    // True if disabled.
    disabled?: boolean;

    // Triggered when the value is changed.
    onChange?: (value: boolean | undefined) => void;
}

export const Toggle = (props: ToggleProps) => {
    const { indeterminate, onChange } = props;

    let value = props.value;

    if (isUndefined(value) && !indeterminate) {
        value = false;
    }

    const [internalValue, setInternalValue] = useState<boolean | undefined>(value);

    useEffect(() => {
        setInternalValue(value);
    }, [value]);

    const doToggle = () => {
        let newValue: boolean | undefined = false;

        if (internalValue) {
            newValue = false;
        } else if (isBoolean(internalValue) && indeterminate) {
            newValue = undefined;
        } else {
            newValue = true;
        }

        onChange && onChange(newValue);

        setInternalValue(newValue);
    };

    let clazz = 'notifo-form-toggle';

    if (internalValue) {
        clazz += ' checked';
    } else if (isBoolean(internalValue)) {
        clazz += ' unchecked';
    } else {
        clazz += ' indeterminate';
    }

    return (
        <div class={clazz} onClick={doToggle}>
            <span class='notifo-form-toggle-slider' />
        </div>
    );
};
