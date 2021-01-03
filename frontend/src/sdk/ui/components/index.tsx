/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h, render } from 'preact';

import { NotificationsOptions, SDKConfig, TopicOptions } from '@sdk/shared';
import { NotificationsContainer } from './Notifications';
import { TopicContainer } from './Topic';

export function renderNotifications(element: Element, options: NotificationsOptions, config: SDKConfig) {
    render(<NotificationsContainer config={config} options={options} />, element);
}

export function renderTopic(element: Element, topicPrefix: string, options: TopicOptions, config: SDKConfig) {
    render(<TopicContainer config={config} topicPrefix={topicPrefix} options={options} />, element);
}

export function destroy(element: Element) {
    render(null, element, element);

    element.innerHTML = '';
}
