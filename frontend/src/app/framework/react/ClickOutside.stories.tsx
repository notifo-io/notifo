/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Card, CardBody } from 'reactstrap';
import { ClickOutside } from './ClickOutside';

const meta: Meta<typeof ClickOutside> = {
    component: ClickOutside,
    render: args => {
        return (
            <ClickOutside {...args}>
                <Card>
                    <CardBody>Inner</CardBody>
                </Card>
            </ClickOutside>
        );
    },
};

export default meta;
type Story = StoryObj<typeof ClickOutside>;

export const Default: Story = {
    args: {
        isActive: true,
    },
};