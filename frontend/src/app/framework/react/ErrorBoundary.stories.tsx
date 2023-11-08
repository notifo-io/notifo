/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { ErrorBoundary } from './ErrorBoundary';

type CustomArgs = React.ComponentProps<typeof ErrorBoundary> & { withError?: boolean };

const meta: Meta<CustomArgs> = {
    component: ErrorBoundary,
    argTypes: {
        withError: {
            control: 'boolean',
        },
    } as any,
    render: args => {
        return (
            <ErrorBoundary {...args}>
                {args.withError &&
                    <Inner />
                }
            </ErrorBoundary>
        );
    },
};

const Inner = () => {
    React.useEffect(() => {
        throw new Error('Failed');
    }, []);

    return null;
};

export default meta;
type Story = StoryObj<CustomArgs>;

export const Default: Story = {
    args: {
        withError: false,
    },
};


export const WithError: Story = {
    args: {
        withError: true,
    },
};
