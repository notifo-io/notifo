/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useCallback } from 'preact/hooks';
import { TopicOptions } from './../../shared';
import { TopicState } from './../model';
import { Icon } from './Icon';

export interface TopicButtonProps {
    // The options.
    options: TopicOptions;

    // True when subscribed.
    subscription: TopicState;

    // Invoked when the button is clicked.
    onClick: () => void;
}

export const TopicButton = (props: TopicButtonProps) => {
    const { onClick, options, subscription } = props;

    const doClick = useCallback((event: Event) => {
        onClick && onClick();

        event.preventDefault();
    }, [onClick]);

    if (subscription === 'Pending' || subscription === 'Unknown') {
        return null;
    }

    let type = options.style;

    if (subscription === 'NotSubscribed') {
        type += '_off';
    }

    return (
        <button class='notifo-button notifo-topics-button' onClick={doClick}>
            <Icon type={type} size={24} />
        </button>
    );
};
