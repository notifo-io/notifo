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
}

export const ApiValue = (props: ApiValueProps) => {
    const { value } = props;

    return (
        <InputGroup>
            <Input className='mono' value={value} readOnly />
        </InputGroup>
    );
};
