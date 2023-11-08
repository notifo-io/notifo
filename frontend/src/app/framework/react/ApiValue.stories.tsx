/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { ApiValue } from './ApiValue';

const meta: Meta<typeof ApiValue> = {
    component: ApiValue,
};

export default meta;
type Story = StoryObj<typeof ApiValue>;

export const Default: Story = {
    args: {
        value: 'API Key',
    },
};
