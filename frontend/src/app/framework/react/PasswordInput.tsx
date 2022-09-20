/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputProps } from 'reactstrap';
import { useBoolean } from './hooks';

export const PasswordInput = (props: InputProps) => {
    const { type: unused, ...otherProps } = props;

    const [isClear, setIsClear] = useBoolean();

    return (
        <div className='input-container'>
            <Input type={isClear ? 'text' : 'password'} {...otherProps}
                spellcheck='false'
                autocorrect='off'
                autocomplete='none' />

            <Button size={otherProps.bsSize} color='link' className={`input-btn input-btn-${otherProps.bsSize}`} onClick={setIsClear.toggle} tabIndex={-1}>
                {isClear ? (
                    <i className='icon-visibility_off' />
                ) : (
                    <i className='icon-visibility' />
                )}
            </Button>
        </div>
    );
};
