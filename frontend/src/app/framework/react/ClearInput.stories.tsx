/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { ClearInput } from './ClearInput';

export default {
    component: ClearInput,
} as ComponentMeta<typeof ClearInput>;

const Template = (args: any) => {
    const [value, setValue] = React.useState('Password');

    return (
        <ClearInput {...args} value={value} onChange={ev => setValue(ev.target.value)} />
    );
};

export const Default = Template.bind({});
