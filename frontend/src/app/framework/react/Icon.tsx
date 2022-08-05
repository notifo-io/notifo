/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';

export type IconType =
    'add' |
    'alternate_email' |
    'browser' |
    'clear' |
    'code' |
    'create' |
    'dashboard' |
    'delete' |
    'error_outline' |
    'expand_less' |
    'expand_more' |
    'extension' |
    'file_copy' |
    'fullscreen' |
    'fullscreen_exit' |
    'history' |
    'hourglass_empty' |
    'integration' |
    'keyboard_arrow_down' |
    'keyboard_arrow_left' |
    'keyboard_arrow_right' |
    'keyboard_arrow_up' |
    'lock_open' |
    'lock_outline' |
    'mail_outline' |
    'message' |
    'messaging' |
    'mobile' |
    'more' |
    'person_add' |
    'person' |
    'photo_size_select_actual' |
    'publish' |
    'refresh' |
    'search' |
    'send' |
    'settings' |
    'sms' |
    'spinner' |
    'topic';

export interface IconProps {
    // The optional classname.
    className?: string;

    // The icon type.
    type: IconType;
}

export const Icon = (props: IconProps) => {
    const { className, type } = props;

    return (
        <i className={classNames(className, `icon-${type}`)} />
    );
};
