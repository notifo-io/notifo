/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { LanguageSelector } from './LanguageSelector';

const meta: Meta<typeof LanguageSelector> = {
    component: LanguageSelector,
    argTypes: {
        languages: {
            table: {
                disable: true,
            },
        },
    },
};

export default meta;
type Story = StoryObj<typeof LanguageSelector>;

export const Default: Story = {
    args: {
        languages: [
            'de',
            'en',
            'it',
        ],
        language: 'en',
    },
};

export const Single: Story = {
    args: {
        languages: [
            'en',
        ],
        language: 'en',
    },
};

export const Many: Story = {
    args: {
        languages: [
            'de',
            'en',
            'it',
            'es',
            'sv',
        ],
        language: 'en',
    },
};