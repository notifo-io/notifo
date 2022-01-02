/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { JsonDetails } from './JsonDetails';

export default {
    component: JsonDetails,
    argTypes: {
        object: {
            control: 'text',
        },
    },
} as ComponentMeta<typeof JsonDetails>;

const Template = (args: any) => {
    return (
        <JsonDetails {...args} object={JSON.parse(args.object)} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    object: JSON.stringify({
        value: [{
            number: 1,
        }],
    }, undefined, 2),
};
