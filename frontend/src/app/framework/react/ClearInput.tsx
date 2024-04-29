/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputProps } from 'reactstrap';
import { Icon } from './Icon';
import { useEventCallback } from './hooks';

export interface ClearInputProps extends InputProps {
    // True when cleared.
    onClear?: () => void;
}

export const ClearInput = (props: InputProps) => {
    const { bsSize, onClear, ...other } = props;

    const [value, setValue] = React.useState(props.value);

    React.useEffect(() => {
        setValue(props.value);
    }, [props.value]);

    const doClear = useEventCallback(() => {
        setValue(undefined);

        props?.onChange?.({ target: { value: '' } } as React.ChangeEvent<HTMLInputElement>);

        onClear && onClear();
    });

    return (
        <div className='input-container'>
            <Input 
                {...other} 
                value={value} 
                bsSize={bsSize}
                spellCheck={false}
                autoCorrect={'off'}
                autoComplete={'none'} />

            {value &&
                <Button size={bsSize} color='link' className={`input-btn input-btn-${bsSize}`} onClick={doClear} tabIndex={-1}>
                    <Icon type='clear' />
                </Button>
            }
        </div>
    );
};
