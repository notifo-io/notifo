/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { NotifoNotification, SDKConfig } from '@sdk/shared';
import { HandleConnect, Connection, HandleDeletion, HandleNotifications } from './connection';

type Update = {
    continuationToken: string;

    notifications: NotifoNotification[];

    deletions?: string[];
};

export class PollingConnection implements Connection {
    private readonly handlersConnect: HandleConnect[] = [];
    private readonly handlersDisconnect: HandleConnect[] = [];
    private readonly handlersNotifications: HandleNotifications[] = [];
    private readonly handlersDeletion: HandleDeletion[] = [];
    private pendingDeleted: string[] = [];
    private pendingSeen: string[] = [];
    private pendingConfirmed: string[] = [];
    private isUpdatePoll = false;
    private connectionStatus: 'Pending' | 'Connected' | 'Disconnected' = 'Pending';
    private connectPromise: Promise<any> | null = null;
    private connectPromiseResolver?: (connect: boolean) => void;
    private continuationToken?: string | null;

    constructor(
        private readonly config: SDKConfig,
    ) {
    }

    public start() {
        if (!this.connectPromise) {
            this.connectPromise = new Promise(resolve => {
                this.connectPromiseResolver = resolve;
            });

            this.poll();
        }

        return this.connectPromise;
    }

    private async poll() {
        const url = `${this.config.apiUrl}/api/me/web/poll`;

        try {
            const body: any = {
                token: this.continuationToken,
            };

            if (this.pendingConfirmed.length > 0) {
                body['confirmed'] = this.pendingConfirmed;
            }

            if (this.pendingSeen.length > 0) {
                body['seen'] = this.pendingSeen;
            }

            if (this.pendingDeleted.length > 0) {
                body['deleted'] = this.pendingDeleted;
            }

            const request: RequestInit = {
                method: 'POST',
                headers: {
                    ...getAuthHeader(this.config),
                    'Content-Type': 'text/json',
                },
                body: JSON.stringify(body),
            };

            const response = await fetch(url, request);

            if (!response.ok) {
                throw new Error('Request failed.');
            }

            const json: Update = await response.json();

            if (this.connectionStatus !== 'Connected') {
                this.connectionStatus = 'Connected';

                for (const callback of this.handlersConnect) {
                    callback();
                }
            }

            if (this.connectPromiseResolver) {
                this.connectPromiseResolver(true);
                this.connectPromiseResolver = undefined;
            }

            if (json.deletions && json.deletions?.length > 0) {
                for (const id of json.deletions) {
                    const payload = { id };

                    for (const callback of this.handlersDeletion) {
                        callback(payload);
                    }
                }
            }

            if (json.notifications.length > 0) {
                for (const callback of this.handlersNotifications) {
                    callback(json.notifications, this.isUpdatePoll);
                }
            }

            this.isUpdatePoll = true;

            if (this.pendingConfirmed.length > 0) {
                this.pendingConfirmed = [];
            }

            if (this.pendingSeen.length > 0) {
                this.pendingSeen = [];
            }

            if (this.pendingDeleted.length > 0) {
                this.pendingDeleted = [];
            }

            this.continuationToken = json.continuationToken;

            setTimeout(() => {
                this.poll();
            }, this.config.pollingInterval);
        } catch (ex) {
            if (this.connectionStatus !== 'Disconnected') {
                this.connectionStatus = 'Disconnected';

                for (const callback of this.handlersDisconnect) {
                    callback();
                }
            }

            setTimeout(() => {
                this.poll();
            }, this.config.pollingInterval * 2);
        }
    }

    public onNotifications(handler: HandleNotifications) {
        this.handlersNotifications.push(handler);
    }

    public onDelete(handler: HandleDeletion) {
        this.handlersDeletion.push(handler);
    }

    public onReconnected(handler: HandleConnect) {
        this.handlersConnect.push(handler);
    }

    public onDisconnected(handler: HandleConnect) {
        this.handlersDisconnect.push(handler);
    }

    public delete(id: string) {
        this.pendingDeleted.push(id);

        const payload = { id };

        for (const callback of this.handlersDeletion) {
            callback(payload);
        }
    }

    public confirmMany(seen: string[], confirmed: string | null | undefined = null) {
        for (const id of seen) {
            this.pendingSeen.push(id);
        }

        if (confirmed) {
            this.pendingConfirmed.push(confirmed);
        }
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
