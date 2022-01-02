/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { StatisticsLabel } from './StatisticsLabel';

export default {
    component: StatisticsLabel,
} as ComponentMeta<typeof StatisticsLabel>;

const Template = (args: any) => {
    return (
        <div style={{ width: 200 }}>
            <StatisticsLabel {...args} />
        </div>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    icon: 'email',
    name: 'Emails',
    total: 10,
    totalAttempt: 100,
    totalFailed: 5,
};

export const SummaryOnly = Template.bind({});

SummaryOnly['args'] = {
    icon: 'email',
    name: 'Emails',
    total: 10,
};

export const NoIcon = Template.bind({});

NoIcon['args'] = {
    name: 'Emails',
    total: 10,
    totalAttempt: 100,
    totalFailed: 5,
};
