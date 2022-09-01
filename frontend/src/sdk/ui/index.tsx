/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { render } from 'preact';
import { buildNotificationsOptions, buildTopicOptions, isString, loadStyle, logError, NotificationsOptions, SDKConfig, TopicOptions } from '@sdk/shared';

import { renderNotificationsUI, renderTopicUI, renderWebPushUI } from './components';

export interface UIOptions {
    style: string;
}

export module UI {
    export async function setupNotifications(elementOrId: string | HTMLElement, opts: NotificationsOptions, config: SDKConfig) {
        const options = buildNotificationsOptions(opts);

        const element = findElement(elementOrId);

        if (!element) {
            return;
        }

        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        renderNotificationsUI(element, options, config);
    }

    export async function setupTopic(elementOrId: string | HTMLElement, topic: string, opts: TopicOptions, config: SDKConfig) {
        const options = buildTopicOptions(opts);

        const element = findElement(elementOrId);

        if (!element) {
            return;
        }

        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        renderTopicUI(element, topic, options, config);
    }

    export async function askForWebPush(config: SDKConfig): Promise<boolean> {
        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        function now() {
            return new Date().getTime();
        }

        function getLastDenied() {
            try {
                return parseInt(localStorage.getItem('notifo-denied') || '0', 10);
            } catch {
                return now();
            }
        }

        function denied() {
            try {
                localStorage.setItem('notifo-denied', now().toString());
            } catch {
                return;
            }
        }

        return await new Promise((resolve) => {
            const lastDenied = getLastDenied();

            if (config.permissionDeniedLifetimeHours > 0) {
                const hoursSinceLastDenied = (now() - lastDenied) / MILLISECONDS_PER_HOUR;

                if (hoursSinceLastDenied < config.permissionDeniedLifetimeHours) {
                    resolve(false);
                    return;
                }
            }

            const element = document.body.appendChild(document.createElement('div'));

            const doAllow = () => {
                resolve(true);

                destroy(element);
            };

            const doDeny = () => {
                resolve(false);

                denied();
                destroy(element);
            };

            renderWebPushUI(element, config, doAllow, doDeny);
        });
    }

    export function destroy(elementOrId: string | HTMLElement) {
        const element = findElement(elementOrId);

        if (!element) {
            return Promise.resolve();
        }

        render(null, element, element);

        element.innerHTML = '';

        return Promise.resolve();
    }
}

function findElement(elementOrId: string | HTMLElement) {
    let element: HTMLElement | null;

    if (isString(elementOrId)) {
        element = document.getElementById(elementOrId);
    } else {
        element = elementOrId;
    }

    if (!element || !element.parentElement) {
        if (isString(elementOrId)) {
            logError(`Cannot find element ${elementOrId}`);
        } else {
            logError('NOTIFO SDK: Cannot find element');
        }

        return null;
    }

    return element;
}

const MILLISECONDS_PER_HOUR = 1000 * 60 * 60;