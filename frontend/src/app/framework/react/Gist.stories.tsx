/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { Gist } from './Gist';

const meta: Meta<typeof Gist> = {
    component: Gist,
};

export default meta;
type Story = StoryObj<typeof Gist>;

export const Default: Story = {
    args: {
        id: '07c756be819ba30f83a27775cdd78dc2',
    },
};