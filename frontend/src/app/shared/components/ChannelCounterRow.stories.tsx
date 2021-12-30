/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { ChannelCounterRow } from './ChannelCounterRow';

export default {
    component: ChannelCounterRow,
    argTypes: {
        channel: {
            table: {
                disable: true,
            },
        },
        counters: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof ChannelCounterRow>;

const Template = (args: any) => {
    return (
        <table style={{ width: 400, tableLayout: 'fixed' }}>
            <ChannelCounterRow {...args} />
        </table>
    );
};

export const Default = Template.bind({});

Default.args = {
    channel: 'email',
    counters: {
        email_handled: 100,
        email_failed: 20,
        email_attempt: 400,
    },
};

export const NoFailed = Template.bind({});

NoFailed.args = {
    channel: 'email',
    counters: {
        email_handled: 100,
        email_attempt: 400,
    },
};

export const NoAttempt = Template.bind({});

NoAttempt.args = {
    channel: 'email',
    counters: {
        email_handled: 100,
        email_failed: 20,
    },
};

export const LargeValues = Template.bind({});

LargeValues.args = {
    channel: 'email',
    counters: {
        email_handled: 100000,
        email_failed: 20000000,
        email_attempt: 40000000000,
    },
};
