/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Timeline, TimelineItem } from './Timeline';

export default {
    component: Timeline,
} as ComponentMeta<typeof Timeline>;

const Template = (args: any) => {
    return (
        <div style={{ background: 'white', padding: '4rem' }}>
            <Timeline {...args} />
        </div>
    );
};

const items: TimelineItem[] = [];

for (let i = 0; i < 10; i++) {
    items.push({ date: (new Date().getTime() / 1000) + (i * 4123), text: `Text ${i}` });
}

export const Empty = Template.bind({});

Empty['args'] = {
    items: null,
};

export const Default = Template.bind({});

Default['args'] = {
    items,
};