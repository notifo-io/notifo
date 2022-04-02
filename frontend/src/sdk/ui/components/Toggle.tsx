/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h } from 'preact';
import { useEffect, useState } from 'preact/hooks';
import { isBoolean, isUndefined } from '@sdk/shared';

export interface ToggleProps {
    // The current value.
    value?: boolean;

    // Set to allow three states.
    indeterminate?: boolean;

    // The field name.
    name: string;

    // True if disabled.
    disabled?: boolean;

    // Triggered when the value is changed.
    onChange?: (value: boolean | undefined, name: string) => void;
}

export const Toggle = (props: ToggleProps) => {
    const { indeterminate, name, onChange } = props;

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

        onChange && onChange(newValue, name);

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
