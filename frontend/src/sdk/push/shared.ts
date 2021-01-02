/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { SDKConfig } from '@sdk/shared';

export type SWMessage =
    SWRegisterMessage |
    SWUnregisterMessage;

export interface SWRegisterMessage extends SWMessageBase {
    type: 'subscribe';
}

export interface SWUnregisterMessage extends SWMessageBase {
    type: 'unsubscribe';
}

export interface SWMessageBase {
    type: 'subscribe' | 'unsubscribe';

    config: SDKConfig;
}
