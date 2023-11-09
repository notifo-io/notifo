/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { FormControlError } from './FormControlError';

const meta: Meta<typeof FormControlError> = {
    component: FormControlError,
    argTypes: {
        error: {
            control: 'text',
        },
    },
    render: args => {
        return (
            <div style={{ paddingTop: 20 }}>
                <FormControlError {...args} />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof FormControlError>;

export const Default: Story = {
    args: {
        error: 'Error',
        touched: true,
    },
};
