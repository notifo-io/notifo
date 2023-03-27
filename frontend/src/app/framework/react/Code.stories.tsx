/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Code } from './Code';

export default {
    component: Code,
} as ComponentMeta<typeof Code>;

const Template = (args: any) => {
    return (
        <Code {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    value: 'Sample Code',
};

export const AuthHeight = Template.bind({});

AuthHeight['args'] = {
    value: 'Sample Code',
    autoHeight: true,
};