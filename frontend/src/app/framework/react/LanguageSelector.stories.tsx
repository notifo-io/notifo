/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { LanguageSelector } from './LanguageSelector';

export default {
    component: LanguageSelector,
    argTypes: {
        languages: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof LanguageSelector>;

const Template = (args: any) => {
    return (
        <LanguageSelector {...args} />
    );
};

export const Default = Template.bind({});

Default['args'] = {
    languages: [
        'de',
        'en',
        'it',
    ],
    language: 'en',
};

export const Single = Template.bind({});

Single['args'] = {
    languages: [
        'en',
    ],
    language: 'en',
};

export const Many = Template.bind({});

Many['args'] = {
    languages: [
        'de',
        'en',
        'it',
        'es',
        'sv',
    ],
    language: 'en',
};
