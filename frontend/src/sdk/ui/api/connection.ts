/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { NotifoNotificationDto } from '@sdk/shared';

export type HandleConnect = () => void;
export type HandleNotifications = (notifications: ReadonlyArray<NotifoNotificationDto>, isUpdate: boolean) => void;
export type HandleDeletion = (request: { id: string }) => void;

export interface Connection {
    start(): Promise<any>;

    onNotifications(handler: HandleNotifications): void;

    onDelete(handler: HandleDeletion): void;

    onReconnected(handler: HandleConnect): void;

    onDisconnected(handler: HandleConnect): void;

    delete(id: string): Promise<any>;

    confirmMany(seen: string[], confirmed?: string | null): Promise<any>;
}
