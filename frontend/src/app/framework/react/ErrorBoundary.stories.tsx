/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { ErrorBoundary } from './ErrorBoundary';

export default {
    component: ErrorBoundary,
    argTypes: {
        withError: {
            control: 'boolean',
        },
    },
} as ComponentMeta<typeof ErrorBoundary>;

const Inner = () => {
    React.useEffect(() => {
        throw new Error('Failed');
    }, []);

    return null;
};

const Template = (args: any) => {
    return (
        <ErrorBoundary {...args}>
            {args.withError &&
                <Inner />
            }
        </ErrorBoundary>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    withError: false,
};

export const WithError = Template.bind({});

WithError['args'] = {
    withError: true,
};
