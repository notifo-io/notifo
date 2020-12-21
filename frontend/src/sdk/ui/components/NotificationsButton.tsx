/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useCallback } from 'preact/hooks';
import { NotificationsOptions } from './../../shared';
import { Icon } from './Icon';

export interface NotificationsButtonProps {
    // The options.
    options: NotificationsOptions;

    // The number of unseen messages.
    unseen?: number;

    // Invoked when the button is clicked.
    onClick: () => void;
}

export const NotificationsButton = (props: NotificationsButtonProps) => {
    const { onClick, options, unseen } = props;

    const doClick = useCallback((event: Event) => {
        onClick && onClick();

        event.preventDefault();
    }, [onClick]);

    return (
        <button type='button' class='notifo-button notifo-notifications-button' onClick={doClick}>
            <Icon type={options.style} size={24} />

            {!!unseen &&
                <div class='notifo-label'>
                    {unseen}
                </div>
            }
        </button>
    );
};
