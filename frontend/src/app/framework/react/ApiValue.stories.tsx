/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { ApiValue } from './ApiValue';

export default {
    component: ApiValue,
} as ComponentMeta<typeof ApiValue>;

const Template = (args: any) => {
    return (
        <ApiValue {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    value: 'API Key',
};
