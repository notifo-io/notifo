/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { ClearInput } from './ClearInput';

const meta: Meta<typeof ClearInput> = {
    component: ClearInput,
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [value, setValue] = React.useState('Password');
    
        return (
            <ClearInput {...args} value={value} onChange={ev => setValue(ev.target.value)} />
        );

    },
};

export default meta;
type Story = StoryObj<typeof ClearInput>;

export const Default: Story = {};