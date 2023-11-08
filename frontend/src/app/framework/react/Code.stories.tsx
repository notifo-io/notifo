/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { Code } from './Code';

const meta: Meta<typeof Code> = {
    component: Code,
};

export default meta;
type Story = StoryObj<typeof Code>;

export const Default: Story = {
    args: {
        value: 'Sample Code',
    },
};

export const AutoHeight: Story = {
    args: {
        value: 'Sample Code',
        autoHeight: true,
    },
};