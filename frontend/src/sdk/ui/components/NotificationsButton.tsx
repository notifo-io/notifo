/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { NotificationsOptions } from '@sdk/shared';
import { getUnseen, useStore } from '@sdk/ui/model';
import { useCallback } from 'preact/hooks';
import { Icon } from './Icon';

export interface NotificationsButtonProps {
    // The options.
    options: NotificationsOptions;

    // Invoked when the button is clicked.
    onClick: () => void;
}

export const NotificationsButton = (props: NotificationsButtonProps) => {
    const { onClick, options } = props;

    const unseen = useStore(x => getUnseen(x));

    const doClick = useCallback((event: Event) => {
        onClick && onClick();

        event.preventDefault();
    }, [onClick]);

    return (
        <button type='button' class='notifo-button notifo-notifications-button' onClick={doClick}>
            <Icon type={options.style} size={24} />

            {!!unseen &&
                <div class='notifo-seen-label'>
                    {unseen}
                </div>
            }
        </button>
    );
};
