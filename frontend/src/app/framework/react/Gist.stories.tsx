/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { Gist } from './Gist';

export default {
    component: Gist,
} as ComponentMeta<typeof Gist>;

const Template = (args: any) => {
    return (
        <Gist {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    id: '07c756be819ba30f83a27775cdd78dc2',
};
