/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { ListPager } from './ListPager';

export default {
    component: ListPager,
    argTypes: {
        list: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof ListPager>;

const Template = (args: any) => {
    return (
        <ListPager {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    list: {
        items: buildItems(5),
        total: 100,
        pageSize: 10,
        page: 5,
    },
};

export const First = Template.bind({});

First['args'] = {
    list: {
        items: buildItems(5),
        total: 100,
        pageSize: 10,
        page: 0,
    },
};

export const Last = Template.bind({});

Last['args'] = {
    list: {
        items: buildItems(5),
        total: 100,
        pageSize: 10,
        page: 9,
    },
};

export const NoTotal = Template.bind({});

NoTotal['args'] = {
    list: {
        items: buildItems(20),
        total: 0,
        pageSize: 10,
        page: 9,
    },
};

function buildItems(length: number) {
    const result: number[] = [];

    for (let i = 0; i < length; i++) {
        result.push(i);
    }

    return result;
}
