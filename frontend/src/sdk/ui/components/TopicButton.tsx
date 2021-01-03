/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { Subscription, TopicOptions } from '@sdk/shared';
import { useCallback } from 'preact/hooks';
import { Icon } from './Icon';

export interface TopicButtonProps {
    // The options.
    options: TopicOptions;

    // True when subscribed.
    subscription: Subscription | null;

    // Invoked when the button is clicked.
    onClick: () => void;
}

export const TopicButton = (props: TopicButtonProps) => {
    const {
        onClick,
        options,
        subscription,
    } = props;

    const doClick = useCallback((event: Event) => {
        onClick && onClick();

        event.preventDefault();
    }, [onClick]);

    let type = options.style;

    if (subscription === null) {
        type += '_off';
    }

    return (
        <button class='notifo-button notifo-topics-button' onClick={doClick}>
            <Icon type={type} size={24} />
        </button>
    );
};
