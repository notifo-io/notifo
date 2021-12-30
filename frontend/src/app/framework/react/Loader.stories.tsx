/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { Loader } from './Loader';

export default {
    component: Loader,
    args: {
        visible: true,
    },
    argTypes: {
        visible: {
            control: 'boolean',
        },
    },
} as ComponentMeta<typeof Loader>;

const Template = (args: any) => {
    const background = args.light ? 'black' : 'white';

    return (
        <div style={{ background, border: '1px solid #eee', padding: '10px 20px' }}>
            <Loader {...args} />
        </div>
    );
};

export const Default = Template.bind({});

Default.args = {
    small: false,
};

export const Small = Template.bind({});

Small.args = {
    small: true,
};
