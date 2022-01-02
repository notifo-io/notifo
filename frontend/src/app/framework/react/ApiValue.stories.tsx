/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
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
