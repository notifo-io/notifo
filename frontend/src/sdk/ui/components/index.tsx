/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { h, render } from 'preact';
import { NotificationsOptions, SDKConfig, TopicOptions } from '@sdk/shared';
import { NotificationsUI } from './NotificationsUI';
import { TopicUI } from './TopicUI';
import { WebPushUI } from './WebPushUI';

export function renderNotificationsUI(element: Element, options: NotificationsOptions, config: SDKConfig) {
    render(<NotificationsUI config={config} options={options} />, element);
}

export function renderTopicUI(element: Element, topicPrefix: string, options: TopicOptions, config: SDKConfig) {
    render(<TopicUI config={config} topicPrefix={topicPrefix} options={options} />, element);
}

export function renderWebPushUI(element: Element, config: SDKConfig, onAllow: () => void, onDeny: () => void) {
    render(<WebPushUI config={config} onAllow={onAllow} onDeny={onDeny} />, element);
}

export function destroy(element: Element) {
    render(null, element, element);

    element.innerHTML = '';
}
