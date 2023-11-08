/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Button, DropdownItem, DropdownMenu } from 'reactstrap';
import { OverlayDropdown } from './OverlayDropdown';

const meta: Meta<typeof OverlayDropdown> = {
    component: OverlayDropdown,
    argTypes: {
        button: {
            table: {
                disable: true,
            },
        },
    },
    render: args => {
        return (
            <div id="portals">
                <div className='text-center'>
                    <OverlayDropdown {...args} button={
                        <Button color='primary'>
                            Open
                        </Button>
                    }>
                        <DropdownMenu>
                            <DropdownItem>Item</DropdownItem>
                        </DropdownMenu>
                    </OverlayDropdown>
                </div>
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof OverlayDropdown>;

export const Default: Story = {};
