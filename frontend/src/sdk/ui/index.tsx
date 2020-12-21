/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { render } from 'preact';
import { buildNotificationsOptions, buildTopicOptions, NotificationsOptions, SDKConfig, TopicOptions } from './../shared';
import { isString, loadStyle, logError } from './../utils';
import { renderNotifications, renderTopic } from './components';

export interface UIOptions {
    style: string;
}

export module UI {
    export async function setupNotifications(elementOrId: string | HTMLElement, opts: NotificationsOptions, config: SDKConfig) {
        const options = buildNotificationsOptions(opts, config);

        const element = findElement(elementOrId);

        if (!element) {
            return;
        }

        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        renderNotifications(element, options, config);
    }

    export async function setupTopic(elementOrId: string | HTMLElement, topic: string, opts: TopicOptions, config: SDKConfig) {
        const options = buildTopicOptions(opts, config);

        const element = findElement(elementOrId);

        if (!element) {
            return;
        }

        if (config.styleUrl) {
            await loadStyle(config.styleUrl);
        }

        renderTopic(element, topic, options, config);
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
    let element: HTMLElement;

    if (isString(elementOrId)) {
        element = document.getElementById(elementOrId);
    } else {
        element = elementOrId;
    }

    if (!element || !element.parentElement) {
        if (isString(elementOrId)) {
            logError(`Cannot find element ${elementOrId}`);
        } else {
            logError(`NOTIFO SDK: Cannot find element`);
        }

        return null;
    }

    return element;
}
