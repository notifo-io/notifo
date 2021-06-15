/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { SDKConfig } from '@sdk/shared';
import { PollingConnection } from './polling-connection';
import { SignalRConnection } from './signalr-connection';

export function buildConnection(config: SDKConfig) {
    if (config.connect === 'signalr') {
        return new SignalRConnection(config);
    } else {
        return new PollingConnection(config);
    }
}
