/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { CodeEditor } from './CodeEditor';

const meta: Meta<typeof CodeEditor> = {
    component: CodeEditor,
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [value, setValue] = React.useState('Code');
    
        return (
            <CodeEditor {...args} value={value} onChange={setValue} />
        );

    },
};

export default meta;
type Story = StoryObj<typeof CodeEditor>;

export const Default: Story = {};