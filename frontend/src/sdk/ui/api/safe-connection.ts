/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { HandleConnect, Connection, HandleDeletion, HandleNotifications } from './connection';

export class SafeConnection implements Connection {
    private readonly deletions: { [id: string]: true } = {};
    private readonly received: { [id: string]: string } = {};

    constructor(
        private readonly inner: Connection,
    ) {
    }

    public start() {
        return this.inner.start();
    }

    public onNotifications(handler: HandleNotifications) {
        this.inner.onNotifications((notifications, isUpdate) => {
            const updates = notifications.filter(x => this.isValid(x.id, x.updated!));

            if (updates.length > 0) {
                // eslint-disable-next-line no-console
                console.debug(`NOTIFO SDK: ${updates.length} new updates received.`);

                handler(updates, isUpdate);
            }
        });
    }

    public onDelete(handler: HandleDeletion) {
        this.inner.onDelete(deletion => {
            if (this.deletions[deletion.id]) {
                this.deletions[deletion.id] = true;

                // eslint-disable-next-line no-console
                console.debug('NOTIFO SDK: 1 new deletion received.');

                handler(deletion);
            }
        });
    }

    public onReconnected(handler: HandleConnect) {
        this.inner.onReconnected(handler);
    }

    public onDisconnected(handler: HandleConnect) {
        this.inner.onDisconnected(handler);
    }

    public delete(id: string) {
        this.inner.delete(id);
    }

    public confirmMany(seen: string[], confirmed: string | null | undefined = null) {
        this.inner.confirmMany(seen, confirmed);
    }

    private isValid(id: string, update: string) {
        let isValid = false;

        if (!this.deletions[id]) {
            const lastUpdate = this.received[id];

            isValid = !lastUpdate || update.localeCompare(lastUpdate) > 0;
        }

        this.received[id] = update;

        return isValid;
    }
}
