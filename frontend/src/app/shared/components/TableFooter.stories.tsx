/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { TableFooter } from './TableFooter';

const meta: Meta<typeof TableFooter> = {
    component: TableFooter,
    argTypes: {
        list: {
            table: {
                disable: true,
            },
        },
    },
};

export default meta;
type Story = StoryObj<typeof TableFooter>;

export const Default: Story = {
    args: {
        list: {
            items: buildItems(5),
            total: 100,
            pageSize: 10,
            page: 5,
        },
    },
};

export const First: Story = {
    args: {
        list: {
            items: buildItems(5),
            total: 100,
            pageSize: 10,
            page: 0,
        },
    },
};

export const Last: Story = {
    args: {
        list: {
            items: buildItems(5),
            total: 100,
            pageSize: 10,
            page: 9,
        },
    },
};

export const NoTotal: Story = {
    args: {
        list: {
            items: buildItems(20),
            total: 0,
            pageSize: 10,
            page: 9,
        },
    },
};

function buildItems(length: number) {
    const result: number[] = [];

    for (let i = 0; i < length; i++) {
        result.push(i);
    }

    return result;
}
