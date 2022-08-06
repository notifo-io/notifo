import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Button, DropdownItem, DropdownMenu } from 'reactstrap';
import { OverlayDropdown } from './OverlayDropdown';

export default {
    component: OverlayDropdown,
    argTypes: {
        button: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof OverlayDropdown>;

const Template = (args: any) => {
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
};

export const Default = Template.bind({});
