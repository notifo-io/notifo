/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as signalR from '@microsoft/signalr';
import { SDKConfig } from '@sdk/shared';
import { Connection, HandleConnect, HandleDeletion, HandleNotifications } from './connection';

class Retry implements signalR.IRetryPolicy {
    private readonly timeouts = [1000, 5000, 10000, 30000];

    public nextRetryDelayInMilliseconds(retryContext: signalR.RetryContext): number | null {
        return this.timeouts[retryContext.previousRetryCount] || 30000;
    }
}

export class SignalRConnection implements Connection {
    private readonly signalR: signalR.HubConnection;

    constructor(config: SDKConfig, negotiate: boolean) {
        const signalRConfig: signalR.IHttpConnectionOptions = {
            headers: {
                ...getAuthHeader(config),
            },
            accessTokenFactory: () => {
                return config.userToken!;
            },
            withCredentials: false,
        };

        if (!negotiate) {
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

    public onNotifications(handler: HandleNotifications) {
        this.signalR.on('notifications', notifications => {
            handler(notifications, false);
        });

        this.signalR.on('notification', notification => {
            handler([notification], true);
        });
    }

    public onDelete(handler: HandleDeletion) {
        this.signalR.on('notificationDeleted', handler);
    }

    public onReconnected(handler: HandleConnect) {
        this.signalR.onreconnected(handler);
    }

    public onDisconnected(handler: HandleConnect) {
        this.signalR.onreconnecting(handler);
    }

    public delete(id: string) {
        return this.signalR.send('delete', id);
    }

    public confirmMany(seen: string[], confirmed: string | null | undefined = null) {
        return this.signalR.send('confirmMany', { confirmed, seen });
    }
}

function getAuthHeader(config: SDKConfig): Record<string, string> {
    if (config.userToken) {
        return {
            'X-ApiKey': config.userToken,
        };
    } else {
        return {
            'X-ApiKey': config.apiKey!,
        };
    }
}
