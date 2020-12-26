/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { de, enUS } from 'date-fns/locale';
import { isNumber, isString, logWarn } from './../utils';

export const SUPPORTED_LOCALES = {
    ['en']: enUS,
    ['de']: de,
};

const Texts = {
    empty: {
        de: 'Keine Benachrichtigungen vorhanden',
        en: 'You have no notifications yet.',
    },
    subscribe: {
        de: 'Abbonnieren',
        en: 'Subscribe',
    },
    unsubscribe: {
        de: 'Deabbonieren',
        en: 'Unsubscribe',
    },
};

const IS_DEV = window.location.host.indexOf('localhost:3002') >= 0;

export function buildSDKConfig(opts: SDKConfig) {
    const options: SDKConfig = <any>{ ...opts || {} };

    if (!isStringOption(options.apiUrl)) {
        logWarn('init.apiURL must be a string if defined, fallback to default.');
        options.apiUrl = undefined;
    }

    if (!options.apiUrl) {
        options.apiUrl = 'https://app.notifo.io';
    }

    if (!isStringOption(options.styleUrl)) {
        logWarn('init.styleUrl must be a string if defined, fallback to default.');
        options.styleUrl = undefined;
    }

    if (!options.styleUrl && !IS_DEV) {
        options.styleUrl = `${options.apiUrl}/build/notifo-sdk.css`;
    }

    if (!isStringOption(options.serviceWorkerUrl)) {
        logWarn('init.serviceWorkerUrl must be a string if defined.');
        options.serviceWorkerUrl = undefined;
    }

    if (!options.serviceWorkerUrl) {
        options.serviceWorkerUrl = IS_DEV ? '/notifo-sdk-worker.js' : '/notifo-sw.js';
    }

    if (!isStringOption(options.userToken)) {
        logWarn('init.userToken must be a string if defined.');
    }

    if (!isStringOption(options.userEmail)) {
        logWarn('init.userEmail must be a string if defined.');
    }

    if (!isStringOption(options.userName)) {
        logWarn('init.userName must be a string if defined.');
    }

    if (!isStringOption(options.userTimezone)) {
        logWarn('init.userTimezone must be a string if defined.');
    }

    if (!isStringOption(options.userLanguage)) {
        logWarn('init.userLanguage must be a string if defined.');
    }

    if (!isLocaleOption(options.locale)) {
        logWarn(`init.locale must be a valid locale. Allowed: ${Object.keys(SUPPORTED_LOCALES).join(',')}`);
        options.locale = undefined;
    }

    if (!options.locale) {
        options.locale = 'en';
    }

    if (!isStringOption(options.apiUrl) && !isStringOption(options.apiKey)) {
        logWarn('init.apiUrl or init.apIKey must be defined');

        return null;
    }

    return options;
}

export function buildNotificationsOptions(opts: NotificationsOptions, config: SDKConfig) {
    const options: NotificationsOptions = <any>{ ...opts || {} };

    if (!isEnumOption(options.position, SUPPORTED_POSITIONS)) {
        logWarn(`show-notifications.position is not one these values: ${SUPPORTED_POSITIONS.join(', ')}`);
        options.position = undefined;
    }

    if (!options.position) {
        options.position = SUPPORTED_POSITIONS[0];
    }

    if (!isEnumOption(options.style, SUPPORTED_MAIN_STYLES)) {
        logWarn(`show-notifications.style must be one of these values: ${SUPPORTED_MAIN_STYLES.join(',')}`);
        options.style = undefined;
    }

    if (!options.style) {
        options.style = SUPPORTED_MAIN_STYLES[0];
    }

    if (options.queryInterval && (!isNumber(options.queryInterval) || options.queryInterval < 2000)) {
        logWarn(`show-notifications.queryInterval must be a number and greater than 2000ms.`);
        options.queryInterval = undefined;
    }

    if (!options.queryInterval) {
        options.queryInterval = 3000;
    }

    if (!options.textEmpty) {
        options.textEmpty = Texts.empty[config.locale];
    }

    return options;
}

export function buildTopicOptions(opts: TopicOptions, config: SDKConfig) {
    const options: TopicOptions = <any>{ ...opts || {} };

    if (!isEnumOption(options.style, SUPPORTED_TOPIC_STYLES)) {
        logWarn(`show-topic.style must be one of these values: ${SUPPORTED_TOPIC_STYLES.join(',')}`);
        options.style = undefined;
    }

    if (!options.style) {
        options.style = SUPPORTED_TOPIC_STYLES[0];
    }

    if (!options.textUnsubscribe) {
        options.textUnsubscribe = Texts.unsubscribe[config.locale];
    }

    return options;
}

function isStringOption(value: any) {
    return !value || isString(value);
}

function isEnumOption(value: any, allowed: string[]) {
    return !value || allowed.indexOf(value) >= 0;
}

function isLocaleOption(value: any) {
    return isEnumOption(value, Object.keys(SUPPORTED_LOCALES));
}

export interface SDKConfig {
    // The API URL
    apiUrl: string;

    // The API KEY when registration is needed.
    apiKey?: string;

    // The public key for web push encryption.
    publicKey?: string;

    // The email address of the user.
    userEmail?: string;

    // The timezone of the user.
    userTimezone?: string;

    // The language of the user.
    userLanguage?: string;

    // The username.
    userName?: string;

    // The user api token, set on registration.
    userToken?: string;

    // The topics, when the user is registered.
    topics?: string[];

    // The url to the styles.
    styleUrl: string;

    // True to negotiate the connection, otherwise sockets are used.
    negotiate: boolean;

    // A callback that is invoked when a notification is retrieved.
    onNotification?: (notification: any) => void;

    // The url to the service worker.
    serviceWorkerUrl: string;

    // The locale settings for date formatting.
    locale: string;
}

export interface SubscribeOptions {
    existingWorker?: true;
}

type OptionTopicStyle = 'star' | 'heart' | 'alarm' | 'bell';

const SUPPORTED_TOPIC_STYLES: OptionTopicStyle[] = ['star', 'heart', 'bell', 'alarm'];

export interface TopicOptions {
    // The style of the button.
    style: OptionTopicStyle;

    // The subscription text.
    textSubscribe: string;

    // The unsubscribe text.
    textUnsubscribe: string;
}

type OptionsPosition = 'bottom-left' | 'bottom-right';
type OptionMainStyle = 'message' | 'chat' | 'chat_filled' | 'notifo';

const SUPPORTED_POSITIONS: OptionsPosition[] = ['bottom-left', 'bottom-right'];
const SUPPORTED_MAIN_STYLES: OptionMainStyle[] = ['message', 'chat', 'chat_filled', 'notifo'];

export interface NotificationsOptions  {
    // The position of the modal.
    position: OptionsPosition;

    // The style of the button.
    style: OptionMainStyle;

    // The text when no notifications are there.
    textEmpty: string;

    // The interval to query.
    queryInterval: number;
}
