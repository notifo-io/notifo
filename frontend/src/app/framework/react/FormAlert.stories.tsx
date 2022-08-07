/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { FormAlert } from './FormAlert';

export default {
    component: FormAlert,
    argTypes: {
        error: {
            control: 'text',
        },
    },
} as ComponentMeta<typeof FormAlert>;

const Template = (args: any) => {
    return (
        <div style={{ paddingTop: 20 }}>
            <FormAlert {...args} />
        </div>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    text: 'Name cannot be changed later.',
};

export const Multline = Template.bind({});

Multline['args'] = {
    text: 'Name cannot be changed later.\n\nPlease be careful.',
};

export const Lists = Template.bind({});

Lists['args'] = {
    text: 'Name cannot be changed later.\n\n* List Item 1.\n* List Item2\n* List Item 3',
};