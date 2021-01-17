/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as signalR from '@microsoft/signalr';
import { NotifoNotification, SDKConfig } from '@sdk/shared';

class Retry implements signalR.IRetryPolicy {
    private readonly timeouts = [1000, 5000, 1000, 30000];

    public nextRetryDelayInMilliseconds(retryContext: signalR.RetryContext): number | null {
        return this.timeouts[retryContext.previousRetryCount] || 30000;
    }
}

export class Connection {
    private readonly signalR: signalR.HubConnection;

    constructor(
        config: SDKConfig,
    ) {
        const signalRConfig: signalR.IHttpConnectionOptions = {
            headers: {
                ...getAuthHeader(config),
            },
            accessTokenFactory: () => {
                return config.userToken;
            },
            withCredentials: false,
        };

        if (!config.negotiate) {
            signalRConfig.skipNegotiation = true;
            signalRConfig.transport = signalR.HttpTransportType.WebSockets;
        }

        const connection = new signalR.HubConnectionBuilder()
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect(new Retry())
            .withUrl(`${config.apiUrl}/hub`, signalRConfig)
            .build();

        this.signalR = connection;
    }

    public start() {
        return this.signalR.start();
    }

    public onNotification(handler: (notification: NotifoNotification) => void) {
        this.signalR.on('notification', handler);
    }

    public onNotifications(handler: (notifications: ReadonlyArray<NotifoNotification>) => void) {
        this.signalR.on('notifications', handler);
    }

    public onDelete(handler: (request: { id: string }) => void) {
        this.signalR.on('notificationDeleted', handler);
    }

    public onReconnected(handler: () => void) {
        this.signalR.onreconnected(handler);
    }

    public onReconnecting(handler: () => void) {
        this.signalR.onreconnecting(handler);
    }

    public delete(id: string) {
        return this.signalR.send('delete', id);
    }

    public confirmMany(seen: string[], confirmed: string | undefined = null) {
        return this.signalR.send('confirmMany', {
            channel: 'Web',
            confirmed,
            seen,
        });
    }
}

function getAuthHeader(config: SDKConfig) {
    if (config.userToken) {
        return {
            ['X-ApiKey']: config.userToken,
        };
    } else {
        return {
            ['X-ApiKey']: config.apiKey,
        };
    }
}
