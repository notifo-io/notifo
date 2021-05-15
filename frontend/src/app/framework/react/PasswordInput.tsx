/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputProps } from 'reactstrap';

export const PasswordInput = (props: InputProps) => {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    const { type: _, ...otherProps } = props;

    const [isClear, setIsClear] = React.useState(false);

    const doToggleClear = React.useCallback(() => {
        setIsClear(!isClear);
    }, [isClear]);

    const type = isClear ? 'text' : 'password';

    return (
        <div className='input-container'>
            <Input type={type} {...otherProps}
                spellcheck='false'
                autocorrect='off'
                autocomplete='none' />

            <Button size={otherProps.bsSize} color='link' className={`input-btn input-btn-${otherProps.bsSize}`} onClick={doToggleClear} tabIndex={-1}>
                {isClear ? (
                    <i className='icon-visibility_off' />
                ) : (
                    <i className='icon-visibility' />
                )}
            </Button>
        </div>
    );
};
