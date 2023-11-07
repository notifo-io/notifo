/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Toggle } from './Toggle';

const meta: Meta<typeof Toggle> = {
    component: Toggle,
    argTypes: {
        value: {
            control: 'boolean',
        },
    },
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [value, setValue] = React.useState<string | boolean | undefined>(false);
    
        // eslint-disable-next-line react-hooks/rules-of-hooks
        React.useEffect(() => {
            setValue(args.value);
        }, [args.value]);
    
        return (
            <Toggle {...args} value={value} onChange={setValue} />
        );
    },
};

export default meta;
type Story = StoryObj<typeof Toggle>;

export const Default: Story = {};

export const Disabled: Story = {
    args: {
        disabled: true,
    },
};

export const Labelled: Story = {
    args: {
        label: 'My Label',
    },
};