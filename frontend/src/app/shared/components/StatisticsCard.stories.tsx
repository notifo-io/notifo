/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { StatisticsCard } from './StatisticsCard';

export default {
    component: StatisticsCard,
} as ComponentMeta<typeof StatisticsCard>;

const Template = (args: any) => {
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
};

export const Default = Template.bind({});

Default['args'] = {
    attempt: 100,
    failed: 10,
    icon: 'email',
    summary: 10,
    summaryLabel: 'Sent',
    title: 'Emails',
};

export const SummaryOnly = Template.bind({});

SummaryOnly['args'] = {
    summary: 10,
};

export const SummaryLabelOnly = Template.bind({});

SummaryLabelOnly['args'] = {
    summary: 10,
    summaryLabel: 'Sent',
};

export const NoDetails = Template.bind({});

NoDetails['args'] = {
    icon: 'email',
    summary: 10,
    summaryLabel: 'Sent',
    title: 'Emails',
};
