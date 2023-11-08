/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { DropZone } from './DropZone';

const meta: Meta<typeof DropZone> = {
    component: DropZone,
};

export default meta;
type Story = StoryObj<typeof DropZone>;

export const Default: Story = {};
