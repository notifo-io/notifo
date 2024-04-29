/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input, InputGroup, InputGroupAddon } from 'reactstrap';
import { Icon } from './Icon';
import { useClipboard, useEventCallback } from './hooks';

export interface ApiValueProps {
    // The api value.
    value: any;

    // The bootstrap size.
    size?: 'sm' | 'lg';
}

export const ApiValue = (props: ApiValueProps) => {
    const { size, value } = props;

    const clipboard = useClipboard();
    const [copied, setCopied] = React.useState(false);
    
    const doCopy = useEventCallback(() => {
        async function copy() {
            await clipboard(value);
            setCopied(true);

            await delay(1500);
            setCopied(false);
        } 

        copy();
    });

    return (
        <InputGroup size={size}>
            <Input className='mono' value={value} readOnly />
        
            <InputGroupAddon addonType='append'>
                <Button type='button' outline={!copied} color={copied ? 'success' : 'secondary'} className='btn-flat transition-all' onClick={doCopy}>
                    <Icon type={copied ? 'check' : 'file_copy'} />
                </Button>
            </InputGroupAddon>
        </InputGroup>
    );
};

function delay(timeout: number) {
    return new Promise(resolve => {
        setTimeout(resolve, timeout);
    });
}