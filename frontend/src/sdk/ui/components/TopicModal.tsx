/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { SDKConfig, TopicOptions } from './../../shared';
import { Modal } from './Modal';

export interface TopicModalProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: TopicOptions;

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

export const TopicModal = (props: TopicModalProps) => {
    const {
        onClickOutside,
        options,
    } = props;

    return (
        <Modal onClickOutside={onClickOutside} position={options.position}>
        </Modal>
    );
};
