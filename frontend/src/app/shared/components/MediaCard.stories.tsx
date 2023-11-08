/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { MediaDto } from '@app/service';
import { MediaCard } from './MediaCard';

const meta: Meta<typeof MediaCard> = {
    component: MediaCard,
    argTypes: {
        media: {
            table: {
                disable: true,
            },
        },
    },
};

export default meta;
type Story = StoryObj<typeof MediaCard>;

export const Default: Story = {
    args: {
        media: {
            mimeType: 'image/png',
            fileName: 'Image.png',
            fileInfo: '1000x600px',
            fileSize: 11233333,
            type: 'Image',
            url: 'https://picsum.photos/id/237/200/152',
        } as MediaDto,
    },
};

export const SmallerImage: Story = {
    args: {
        media: {
            mimeType: 'image/png',
            fileName: 'Image.png',
            fileInfo: '1000x600px',
            fileSize: 11233333,
            type: 'Image',
            url: 'https://picsum.photos/id/237/100/150',
        } as MediaDto,
    },
};

export const NoImage: Story = {
    args: {
        media: {
            mimeType: 'image/png',
            fileName: 'Image.png',
            fileInfo: '1000x600px',
            fileSize: 11233333,
            type: 'Image',
        } as MediaDto,
    },
};