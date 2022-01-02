/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { Shortcut } from './Shortcut';

export default {
    component: Shortcut,
} as ComponentMeta<typeof Shortcut>;

const Template = (args: any) => {
    return (
        <>
            {args.keys}

            <Shortcut {...args} />
        </>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    keys: 'ctrl+s',
};
