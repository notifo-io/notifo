/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Input, InputGroup } from 'reactstrap';

export interface ApiValueProps {
    // The api value.
    value: any;

    // The optional label.
    label?: string;

    // The bootstrap size.
    size?: string;
}

export const ApiValue = (props: ApiValueProps) => {
    const { size, value } = props;

    return (
        <InputGroup size={size}>
            <Input className='mono' value={value} readOnly />
        </InputGroup>
    );
};
