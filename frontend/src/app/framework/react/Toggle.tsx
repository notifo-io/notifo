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
    value?: boolean | string;

    // Set to allow three states.
    indeterminate?: boolean;

    // True if disabled.
    disabled?: boolean;

    // The label string.
    label?: string;

    // Save as string.
    asString?: boolean;

    // Triggered when the value is changed.
    onChange?: (value: boolean | undefined | string) => void;
}

export const Toggle = (props: ToggleProps) => {
    const { asString, indeterminate, label, onChange } = props;

    let value = props.value;

    if (Types.isUndefined(value) && !indeterminate) {
        value = false;
    }

    const [internalValue, setInternalValue] = React.useState<boolean | undefined>(getValue(value));

    React.useEffect(() => {
        setInternalValue(getValue(value));
    }, [value]);

    const doToggle = () => {
        let newValue: boolean | undefined | string = false;

        if (internalValue) {
            newValue = false;
        } else if (Types.isBoolean(internalValue) && indeterminate) {
            newValue = undefined;
        } else {
            newValue = true;
        }

        if (onChange) {
            if (asString) {
                onChange && onChange(newValue?.toString());
            } else {
                onChange(newValue);
            }
        }
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
            <span className={clazz} />

            {label &&
                <span className='custom-toggle-label'>{label}</span>
            }
        </label>
    );
};

function getValue(value: boolean | string | undefined) {
    if (Types.isString(value)) {
        return value === '1' || value === 'true' || value === 'True';
    } else {
        return value;
    }
}
