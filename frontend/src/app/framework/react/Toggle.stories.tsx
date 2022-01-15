/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Toggle } from './Toggle';

export default {
    component: Toggle,
    argTypes: {
        value: {
            control: 'boolean',
        },
    },
} as ComponentMeta<typeof Toggle>;

const Template = (args: any) => {
    const [value, setValue] = React.useState(true);

    React.useEffect(() => {
        setValue(args.value);
    }, [args.value]);

    return (
        <Toggle {...args} value={value} onChange={setValue} />
    );
};

export const Default = Template.bind({});

export const Disabled = Template.bind({});

Disabled['args'] = {
    disabled: true,
};

export const Labelled = Template.bind({});

Labelled['args'] = {
    label: 'My Label',
};
