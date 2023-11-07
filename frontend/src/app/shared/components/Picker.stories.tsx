/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Input } from 'reactstrap';
import { Picker } from './Picker';

const meta: Meta<typeof Picker> = {
    component: Picker,
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [value, setValue] = React.useState('');

        return (
            <div id="portals">
                <div className='input-container'>
                    <Input value={value} onChange={e => setValue(e.target.value)} />
    
                    <Picker {...args} onPick={e => setValue(x => x + e)} />
                </div>
            </div>
        );
    },
};


export default meta;
type Story = StoryObj<typeof Picker>;

export const Default: Story = {
    args: {
        pickArgument: true,
        pickEmoji: true,
        pickMedia: true,
    },
};