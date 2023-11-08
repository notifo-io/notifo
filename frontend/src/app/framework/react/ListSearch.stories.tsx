/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { ListSearch } from './ListSearch';

const meta: Meta<typeof ListSearch> = {
    component: ListSearch,
    argTypes: {
        list: {
            table: {
                disable: true,
            },
        },
    },
};

export default meta;
type Story = StoryObj<typeof ListSearch>;

export const Default: Story = {
    args: {
        list: {
            search: 'Search',
            items: [],
            total: 100,
            pageSize: 10,
            page: 5,
        },
    },
};