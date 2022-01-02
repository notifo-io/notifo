/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { FormControlError } from './FormControlError';

export default {
    component: FormControlError,
    argTypes: {
        error: {
            control: 'text',
        },
    },
} as ComponentMeta<typeof FormControlError>;

const Template = (args: any) => {
    return (
        <div style={{ paddingTop: 20 }}>
            <FormControlError {...args} />
        </div>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    error: 'Error',
    touched: true,
};
