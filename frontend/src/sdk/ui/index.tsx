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

    export async function setupTopic(elementOrId: string | HTMLElement, topicPrefix: string, opts: TopicOptions, config: SDKConfig) {
        const options = buildTopicOptions(opts);

        const element = findElement(elementOrId);

        if (!element) {
            return;
        }

        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        renderTopicUI(element, topicPrefix, options, config);
    }

    export async function askForWebPush(config: SDKConfig): Promise<boolean> {
        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        return await new Promise((resolve) => {
            const element = document.body.appendChild(document.createElement('div'));

            const doAllow = () => {
                resolve(true);

                destroy(element);
            };

            const doDeny = () => {
                resolve(false);

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
