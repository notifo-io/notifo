/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { Logo } from './Logo';

export default {
    component: Logo,
} as ComponentMeta<typeof Logo>;

const Template = (args: any) => {
    return (
        <div style={{ width: 100, padding: 10, background: 'black' }}>
            <Logo {...args} />
        </div>
    );
};

export const Default = Template.bind({});
