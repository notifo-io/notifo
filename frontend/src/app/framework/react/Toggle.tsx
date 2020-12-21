/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Types } from './../utils';

export interface ToggleProps {
    // The current value.
    value?: boolean;

    // Set to allow three states.
    indeterminate?: boolean;

    // True if disabled.
    disabled?: boolean;

    // The label string.
    label?: string;

    // Triggered when the value is changed.
    onChange?: (value: boolean | undefined) => void;
}

export const Toggle = (props: ToggleProps) => {
    const { indeterminate, label, onChange } = props;

    let value = props.value;

    if (Types.isUndefined(value) && !indeterminate) {
        value = false;
    }

    const [internalValue, setInternalValue] = React.useState<boolean | undefined>(value);

    React.useEffect(() => {
        setInternalValue(value);
    }, [value]);

    const doToggle = () => {
        let newValue: boolean | undefined = false;

        if (internalValue) {
            newValue = false;
        } else if (Types.isBoolean(internalValue) && indeterminate) {
            newValue = undefined;
        } else {
            newValue = true;
        }

        onChange && onChange(newValue);

        setInternalValue(newValue);
    };

    let clazz = 'custom-toggle-slider';

    if (internalValue) {
        clazz += ' checked';
    } else if (Types.isBoolean(internalValue)) {
        clazz += ' unchecked';
    } else {
        clazz += ' indeterminate';
    }

    return (
        <label className='custom-toggle' onClick={doToggle}>
            <span className={clazz} /> {label}
        </label>
    );
};
