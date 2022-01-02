/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { DropZone } from './DropZone';

export default {
    component: DropZone,
} as ComponentMeta<typeof DropZone>;

const Template = (args: any) => {
    return (
        <DropZone {...args} />
    );
};

export const Default = Template.bind({});
