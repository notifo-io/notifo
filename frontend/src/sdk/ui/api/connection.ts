/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { NotifoNotification } from '@sdk/shared';

export type ConnectHandler = () => void;
export type NotificationHandler = (notification: NotifoNotification) => void;
export type NotificationsHandler = (notifications: ReadonlyArray<NotifoNotification>) => void;
export type DeletionHandler = (request: { id: string }) => void;

export interface Connection {
    start(): Promise<any>;

    onNotification(handler: NotificationHandler): void;

    onNotifications(handler: NotificationsHandler): void;

    onDelete(handler: DeletionHandler): void;

    onReconnected(handler: ConnectHandler): void;

    onReconnecting(handler: ConnectHandler): void;

    delete(id: string): void;

    confirmMany(seen: string[], confirmed: string | null | undefined): void;
}
