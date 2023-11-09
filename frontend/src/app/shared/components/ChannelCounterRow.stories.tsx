/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { ChannelCounterRow } from './ChannelCounterRow';

const meta: Meta<typeof ChannelCounterRow> = {
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
    render: args => {
        return (
            <table style={{ width: 400, tableLayout: 'fixed' }}>
                <ChannelCounterRow {...args} />
            </table>
        );
    },
};

export default meta;
type Story = StoryObj<typeof ChannelCounterRow>;

export const Default: Story = {
    args: {
        channel: 'email',
        counters: {
            email_handled: 100,
            email_failed: 20,
            email_attempt: 400,
        },
    },
};

export const NoFailed: Story = {
    args: {
        channel: 'email',
        counters: {
            email_handled: 100,
            email_attempt: 400,
        },
    },
};

export const NoAttempt: Story = {
    args: {
        channel: 'email',
        counters: {
            email_handled: 100,
            email_failed: 20,
        },
    },
};

export const LargeValues: Story = {
    args: {
        channel: 'email',
        counters: {
            email_handled: 100000,
            email_failed: 20000000,
            email_attempt: 40000000000,
        },
    },
};