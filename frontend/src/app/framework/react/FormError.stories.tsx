/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { FormError } from './FormError';

export default {
    component: FormError,
    argTypes: {
        error: {
            control: 'text',
        },
    },
} as ComponentMeta<typeof FormError>;

const Template = (args: any) => {
    return (
        <FormError {...args} />
    );
};

export const Default = Template.bind({});

Default.args = {
    error: 'Error',
};
