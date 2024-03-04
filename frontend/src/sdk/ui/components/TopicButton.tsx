/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsximportsource preact */

import { useCallback } from 'preact/hooks';
import { SubscriptionDto, TopicOptions } from '@sdk/shared';
import { Icon, IconType } from './Icon';

export interface TopicButtonProps {
    // The options.
    options: TopicOptions;

    // True when subscribed.
    subscription: SubscriptionDto | null;

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

    let icon: IconType = 'heart';
    switch (options.style) {
        case 'alarm':
            icon = !subscription ? 'alarm_off' : 'alarm';
            break;
        case 'bell':
            icon = !subscription ? 'bell' : 'bell_filled';
            break;
        case 'heart':
            icon = !subscription ? 'heart' : 'heart_filled';
            break;
        case 'star':
            icon = !subscription ? 'star' : 'star_filled';
            break;
    }

    return (
        <button class='notifo-button notifo-topics-button' onClick={doClick}>
            <Icon type={icon} size={24} />
        </button>
    );
};
