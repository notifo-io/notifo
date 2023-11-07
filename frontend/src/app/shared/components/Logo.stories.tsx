/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Logo } from './Logo';

const meta: Meta<typeof Logo> = {
    component: Logo,
    render: () => {
        return (
            <div style={{ width: 100, padding: 10, background: 'black' }}>
                <Logo />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof Logo>;

export const Default: Story = {};
