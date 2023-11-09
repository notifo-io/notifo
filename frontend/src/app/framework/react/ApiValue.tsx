/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Input, InputGroup } from 'reactstrap';

export interface ApiValueProps {
    // The api value.
    value: any;

    // The bootstrap size.
    size?: 'sm' | 'lg';
}

export const ApiValue = (props: ApiValueProps) => {
    const { size, value } = props;

    return (
        <InputGroup size={size}>
            <Input className='mono' value={value} readOnly />
        </InputGroup>
    );
};
