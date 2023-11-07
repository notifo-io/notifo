/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import * as React from 'react';
import { Icon, IconType } from './Icon';

const meta: Meta<typeof Icon> = {
    component: Icon,
    render: () => {
        return (
            <>
                {ALLICONS.map(icon =>
                    <div style={{ margin: 10, width: 100, display: 'inline-block' }} className='text-center'>
                        <Icon type={icon} />

                        <small className='truncate'>
                            {icon}
                        </small>
                    </div>,
                )}
            </>
        );
    },
};

export default meta;
type Story = StoryObj<typeof Icon>;

export const Default: Story = {};

const ALLICONS: IconType[] = [
    'add',
    'alternate_email',
    'browser',
    'clear',
    'code',
    'create',
    'dashboard',
    'delete',
    'error_outline',
    'expand_less',
    'expand_more',
    'extension',
    'file_copy',
    'history',
    'hourglass_empty',
    'integration',
    'keyboard_arrow_down',
    'keyboard_arrow_left',
    'keyboard_arrow_right',
    'keyboard_arrow_up',
    'mail_outline',
    'message',
    'messaging',
    'mobile',
    'more',
    'person_add',
    'person',
    'photo_size_select_actual',
    'publish',
    'refresh',
    'search',
    'send',
    'settings',
    'sms',
    'spinner',
];
