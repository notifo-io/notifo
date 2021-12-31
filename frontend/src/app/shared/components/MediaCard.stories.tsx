/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { ComponentMeta } from '@storybook/react';
import { MediaDto } from '@app/service';
import { MediaCard } from './MediaCard';

export default {
    component: MediaCard,
    argTypes: {
        media: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<typeof MediaCard>;

const Template = (args: any) => {
    return (
        <MediaCard {...args} />
    );
};

export const Default = Template.bind({});

Default.args = {
    media: {
        mimeType: 'image/png',
        fileName: 'Image.png',
        fileInfo: '1000x600px',
        fileSize: 11233333,
        type: 'Image',
        url: 'https://picsum.photos/id/237/200/152',
    } as MediaDto,
};

export const SmallerImage = Template.bind({});

SmallerImage.args = {
    media: {
        mimeType: 'image/png',
        fileName: 'Image.png',
        fileInfo: '1000x600px',
        fileSize: 11233333,
        type: 'Image',
        url: 'https://picsum.photos/id/237/100/150',
    } as MediaDto,
};

export const NoImage = Template.bind({});

NoImage.args = {
    media: {
        mimeType: 'image/png',
        fileName: 'Image.png',
        fileInfo: '1000x600px',
        fileSize: 11233333,
        type: 'Image',
    } as MediaDto,
};
