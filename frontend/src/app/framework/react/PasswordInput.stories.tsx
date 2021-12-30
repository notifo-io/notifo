/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
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
