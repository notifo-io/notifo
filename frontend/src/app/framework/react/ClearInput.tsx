/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputProps } from 'reactstrap';
import { Icon } from './Icon';

export const ClearInput = (props: InputProps) => {
    const input = React.useRef<HTMLInputElement | null>(null);

    const [value, setValue] = React.useState(props.value);

    React.useEffect(() => {
        setValue(props.value);
    }, [props.value]);

    const doClear = React.useCallback(() => {
        if (input.current) {
            setValue(undefined);
            setNativeValue(input.current, '');

            input.current.dispatchEvent(new Event('input', { bubbles: true }));
        }
    }, [input.current]);

    const { bsSize } = props;

    return (
        <div className='input-container'>
            <Input value={value} {...props} innerRef={input}
                spellCheck={ false }
                autoCorrect={ 'off' }
                autoComplete={ 'none' } />

            {value &&
                <Button size={bsSize} color='link' className='input-btn' onClick={doClear} tabIndex={-1}>
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
