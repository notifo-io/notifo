/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { FormAlert } from './FormAlert';

const meta: Meta<typeof FormAlert> = {
    component: FormAlert,
    argTypes: {
        text: {
            control: 'text',
        },
    },
    render: args =>{
        return (
            <div style={{ paddingTop: 20 }}>
                <FormAlert {...args} />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof FormAlert>;

export const Default: Story = {
    args: {
        text: 'Name cannot be changed later.',
    },
};

export const Multline: Story = {
    args: {
        text: 'Name cannot be changed later.\n\nPlease be careful.',
    },
};


export const Lists: Story = {
    args: {
        text: 'Name cannot be changed later.\n\n* List Item 1.\n* List Item2\n* List Item 3',
    },
};