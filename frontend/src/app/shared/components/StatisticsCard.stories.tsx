/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { StatisticsCard } from './StatisticsCard';

const meta: Meta<typeof StatisticsCard> = {
    component: StatisticsCard,
    render: args => {
        return (
            <>
                <div style={{ width: 200 }}>
                    <StatisticsCard {...args} />
                </div>
                <div style={{ width: 300, marginTop: 10 }}>
                    <StatisticsCard {...args} />
                </div>
            </>
        );
    },
};

export default meta;
type Story = StoryObj<typeof StatisticsCard>;

export const Default: Story = {
    args: {
        attempt: 100,
        failed: 10,
        icon: 'email' as any,
        summary: 10,
        summaryLabel: 'Sent',
        title: 'Emails',
    },
};

export const SummaryOnly: Story = {
    args: {
        summary: 10,
    },
};

export const SummaryLabelOnly: Story = {
    args: {
        summary: 10,
        summaryLabel: 'Sent',
    },
};

export const NoDetails: Story = {
    args: {
        icon: 'email' as any,
        summary: 10,
        summaryLabel: 'Sent',
        title: 'Emails',
    },
};