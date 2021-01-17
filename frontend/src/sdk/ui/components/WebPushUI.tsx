/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import {  h } from 'preact';

import { SDKConfig } from '@sdk/shared';
import { Modal } from './Modal';

export interface WebPushUIProps {
    // The main config.
    config: SDKConfig;

    // Triggered when confirmed.
    onAllow?: () => void;

    // Triggered when denied.
    onDeny?: () => void;
}

export const WebPushUI = (props: WebPushUIProps) => {
    const {
        config,
        onAllow,
        onDeny,
    } = props;

    return (
        <div class='notifo'>
            <Modal position='top-global'>
                <div class='notifo-form-group'>
                    <h4>{config.texts.webPushConfirmTitle}</h4>
                </div>

                <div class='notifo-form-group'>
                    <small>{config.texts.webPushConfirmText}</small>
                </div>

                <div class='notifo-form-group'>
                    <button class='notifo-form-button primary' onClick={onAllow}>
                        {config.texts.allow}
                    </button>

                    <button class='notifo-form-button' onClick={onDeny}>
                        {config.texts.deny}
                    </button>
                </div>
            </Modal>
        </div>
    );
};
