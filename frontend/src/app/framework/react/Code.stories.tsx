/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { CodeDetails } from './Code';

export default {
    component: CodeDetails,
} as ComponentMeta<typeof CodeDetails>;

const Template = (args: any) => {
    return (
        <CodeDetails {...args} />
    );
};
export const Default = Template.bind({});

Default['args'] = {
    value: 'Sample Code',
};