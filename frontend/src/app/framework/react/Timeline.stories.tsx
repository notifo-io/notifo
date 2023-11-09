/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { Timeline, TimelineItem } from './Timeline';

const meta: Meta<typeof Timeline> = {
    component: Timeline,
    render: args => {
        return (
            <div style={{ background: 'white', padding: '4rem' }}>
                <Timeline {...args} />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof Timeline>;

export const Empty: Story = {
    args: {
        items: null,
    },
};

export const Default: Story = {
    args: {
        items: buildItems(10),
    },
};

function buildItems(count: number) {
    const items: TimelineItem[] = [];
    
    for (let i = 0; i < count; i++) {
        items.push({ date: (new Date().getTime() / 1000) + (i * 4123), text: `Text ${i}` });
    }

    return items;
}