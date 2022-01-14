/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { ListSearch } from './ListSearch';

export default {
    component: ListSearch,
    argTypes: {
        list: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof ListSearch>;

const Template = (args: any) => {
    return (
        <ListSearch {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    list: {
        search: 'Search',
        items: [],
        total: 100,
        pageSize: 10,
        page: 5,
    },
};
