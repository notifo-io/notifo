/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { StatisticsLabel } from './StatisticsLabel';

const meta: Meta<typeof StatisticsLabel> = {
    component: StatisticsLabel,
    render: args => {
        return (
            <div style={{ width: 200 }}>
                <StatisticsLabel {...args} />
            </div>
        );
    },
};

export default meta;
type Story = StoryObj<typeof StatisticsLabel>;

export const Default: Story = {
    args: {
        icon: 'email' as any,
        name: 'Emails',
        total: 10,
        totalAttempt: 100,
        totalFailed: 5,
    },
};

export const SummaryOnly: Story = {
    args: {
        icon: 'email' as any,
        name: 'Emails',
        total: 10,
        totalFailed: undefined,
    },
};

export const NoIcon: Story = {
    args: {
        name: 'Emails',
        total: 10,
        totalAttempt: 100,
        totalFailed: 5,
    },
};