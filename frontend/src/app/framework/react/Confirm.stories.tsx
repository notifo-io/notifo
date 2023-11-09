/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { Button } from 'reactstrap';
import { Confirm } from './Confirm';

const meta: Meta<typeof Confirm> = {
    component: Confirm,
    render: args => {
        return (
            <Confirm {...args}>
                {({ onClick }) => (
                    <Button onClick={onClick} color='danger' outline>Delete?</Button>
                )}
            </Confirm>
        );
    },
};

export default meta;
type Story = StoryObj<typeof Confirm>;

export const Default: Story = {
    args: {
        title: 'Delete item',
        text: undefined,
    },
};

export const WithText: Story = {
    args: {
        title: 'Delete item',
        text: 'Do you want to delete the item?',
    },
};