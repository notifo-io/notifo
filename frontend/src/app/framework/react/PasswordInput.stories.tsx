/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { PasswordInput } from './PasswordInput';

const meta: Meta<typeof PasswordInput> = {
    component: PasswordInput,
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [value, setValue] = React.useState('Password');
    
        return (
            <PasswordInput {...args} value={value} onChange={ev => setValue(ev.target.value)} />
        );
    },
};

export default meta;
type Story = StoryObj<typeof PasswordInput>;

export const Default: Story = {};
