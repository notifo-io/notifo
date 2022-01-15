/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Button } from 'reactstrap';
import { Confirm } from './Confirm';

export default {
    component: Confirm,
} as ComponentMeta<typeof Confirm>;

const Template = (args: any) => {
    return (
        <Confirm {...args}>
            {({ onClick }) => (
                <Button onClick={onClick} color='danger' outline>Delete?</Button>
            )}
        </Confirm>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    title: 'Delete item',
};

export const WithText = Template.bind({});

WithText['args'] = {
    text: 'Do you want to delete the item?',
    ...Default['args'],
};
