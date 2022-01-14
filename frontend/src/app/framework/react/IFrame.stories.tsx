/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { IFrame } from './IFrame';

export default {
    title: 'Framework/React/IFrame',
    component: IFrame,
} as ComponentMeta<typeof IFrame>;

const Template = (args: any, { loaded }: { loaded: any }) => {
    return (
        <IFrame style={{ width: '100%', height: 500 }} {...args} html={loaded.html} />
    );
};

export const Default = Template.bind({});

async function loadPage() {
    let text: string = await (await fetch('https://notifo.io')).text();

    text = text.replace(/href="/g, 'href="https://notifo.io/');
    text = text.replace(/src="/g, 'src="https://notifo.io/');

    return text;
}

Default['loaders'] = [
    async () => ({
        html: await loadPage(),
    }),
];

Default['argTypes'] = {
    html: {
        table: {
            disable: true,
        },
    },
    style: {
        table: {
            disable: true,
        },
    },
};
