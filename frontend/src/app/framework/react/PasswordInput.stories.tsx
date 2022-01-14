/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { PasswordInput } from './PasswordInput';

export default {
    component: PasswordInput,
} as ComponentMeta<typeof PasswordInput>;

const Template = (args: any) => {
    const [value, setValue] = React.useState('Password');

    return (
        <PasswordInput {...args} value={value} onChange={ev => setValue(ev.target.value)} />
    );
};

export const Default = Template.bind({});
