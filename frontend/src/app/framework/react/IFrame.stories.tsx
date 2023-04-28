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

const Template = (args: any) => {
    const [html, setHtml] = React.useState<string>('');

    React.useEffect(() => {
        function delay(timeout: number) {
            return new Promise(resolve => {
                setTimeout(resolve, timeout);
            });
        }

        async function load() {
            while (true) {
                setHtml('<div>Content1</div>');
                await delay(2000);

                setHtml('<div>Content2</div>');
                await delay(2000);
            }
        }

        load();
    }, []);

    return (
        <IFrame style={{ width: '100%', height: 500 }} {...args} html={html} />
    );
};

export const Default = Template.bind({});

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
