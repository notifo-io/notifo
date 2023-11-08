/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { IFrame } from './IFrame';

const meta: Meta<typeof IFrame> = {
    component: IFrame,
    argTypes: {
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
    },
    render: args => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [html, setHtml] = React.useState<string>('');

        // eslint-disable-next-line react-hooks/rules-of-hooks
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
    },
};

export default meta;
type Story = StoryObj<typeof IFrame>;

export const Default: Story = {};
