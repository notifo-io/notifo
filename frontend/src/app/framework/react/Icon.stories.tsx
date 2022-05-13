/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Icon, IconType } from './Icon';

export default {
    component: Icon,
} as ComponentMeta<typeof Icon>;

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

export const Default = () => {
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
};
