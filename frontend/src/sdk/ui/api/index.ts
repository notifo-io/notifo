/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { SDKConfig } from '@sdk/shared';
import { PollingConnection } from './polling-connection';
import { SafeConnection } from './safe-connection';
import { SignalRConnection } from './signalr-connection';

export { Connection } from './connection';

export function buildConnection(config: SDKConfig) {
    if (config.connectionMode === 'SignalR') {
        return new SafeConnection(new SignalRConnection(config, true));
    } else if (config.connectionMode === 'SignalRSockets') {
        return new SafeConnection(new SignalRConnection(config, false));
    } else {
        return new SafeConnection(new PollingConnection(config));
    }
}
