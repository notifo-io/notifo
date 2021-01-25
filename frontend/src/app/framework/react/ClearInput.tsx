/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputProps } from 'reactstrap';
import { Icon } from './Icon';

export interface ClearInputProps extends InputProps {
    // True when cleared.
    onClear?: () => void;
}

export const ClearInput = (props: InputProps) => {
    const { bsSize, onClear, ...other } = props;

    const container = React.useRef<HTMLDivElement | null>(null);

    const [value, setValue] = React.useState(props.value);

    React.useEffect(() => {
        setValue(props.value);
    }, [props.value]);

    const doClear = React.useCallback(() => {
        if (container.current) {
            setValue(undefined);

            const input: HTMLInputElement = container.current.children[0] as HTMLInputElement;

            if (input) {
                setNativeValue(input, '');

                input.dispatchEvent(new Event('input', { bubbles: true }));
            }

            if (onClear) {
                onClear();
            }
        }
    }, [container.current, onClear]);

    return (
        <div className='input-container' ref={container}>
            <Input value={value} {...other} bsSize={bsSize}
                spellCheck={ false }
                autoCorrect={ 'off' }
                autoComplete={ 'none' } />

            {value &&
                <Button size={bsSize} color='link' className={`input-btn input-btn-${bsSize}`} onClick={doClear} tabIndex={-1}>
                    <Icon type='clear' />
                </Button>
            }
        </div>
    );
};

function setNativeValue(element: HTMLInputElement, value: string) {
    const valueSetter = Object.getOwnPropertyDescriptor(element, 'value')!.set;

    const prototype = Object.getPrototypeOf(element);
    const prototypeValueSetter = Object.getOwnPropertyDescriptor(prototype, 'value')!.set;

    if (valueSetter && valueSetter !== prototypeValueSetter) {
        prototypeValueSetter!.call(element, value);
    } else {
        valueSetter!.call(element, value);
    }
}
