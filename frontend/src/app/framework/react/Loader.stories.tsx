/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Loader } from './Loader';

const meta: Meta<typeof Loader> = {
    component: Loader,
    args: {
        visible: true,
    },
    argTypes: {
        visible: {
            control: 'boolean',
        },
    },
    render: args => {
        const background = args.light ? 'black' : 'white';
    
        return (
            <div style={{ background, border: '1px solid #eee', padding: '10px 20px' }}>
                <Loader {...args} />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof Loader>;

export const Default: Story = {
    args: {
        small: false,
    },
};

export const Small: Story = {
    args: {
        small: true,
    },
};