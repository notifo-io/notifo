/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { FormError } from './FormError';

const meta: Meta<typeof FormError> = {
    component: FormError,
    argTypes: {
        error: {
            control: 'text',
        },
    },
};

export default meta;
type Story = StoryObj<typeof FormError>;

export const Default: Story = {
    args: {
        error: 'Error',
    },
};