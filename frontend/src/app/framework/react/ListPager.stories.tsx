/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { ListPager } from './ListPager';

const meta: Meta<typeof ListPager> = {
    component: ListPager,
    argTypes: {
        list: {
            table: {
                disable: true,
            },
        },
    },
};

export default meta;
type Story = StoryObj<typeof ListPager>;

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
