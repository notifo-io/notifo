/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { de, enUS } from 'date-fns/locale';
import { isNumber } from 'lodash';
import { isObject, isString, isUndefined, logWarn } from './utils';

export const SUPPORTED_LOCALES = {
    en: enUS,
    de,
};

const DefaultTexts: Texts<{ de: string; en: string }> = {
    allow: {
        de: 'Erlauben',
        en: 'Allow',
    },
    archive: {
        de: 'Archiv',
        en: 'Archive',
    },
    archiveLink: {
        de: 'Gelöschte Benachrichtungen',
        en: 'Deleted notifications',
    },
    cancel: {
        de: 'Abbrechen',
        en: 'Cancel',
    },
    deny: {
        de: 'Nein, Danke',
        en: 'Deny',
    },
    email: {
        de: 'E-Mail',
        en: 'E-Mail',
    },
    emailAddress: {
        de: 'E-Mail Adresse',
        en: 'E-Mail Address',
    },
    fullName: {
        de: 'Name',
        en: 'Name',
    },
    language: {
        de: 'Sprache',
        en: 'Language',
    },
    loadingFailed: {
        de: 'Laden ist fehlgeschlagen',
        en: 'Loading has failed',
    },
    mobilepush: {
        de: 'Mobile Push',
        en: 'Mobile Push',
    },
    messaging: {
        de: 'Messaging',
        en: 'Messaging',
    },
    notifications: {
        de: 'Nachrichten',
        en: 'Notifications',
    },
    notifyBeEmail: {
        de: 'Benachrichtige mich per Email',
        en: 'Notify me via Email',
    },
    notifyBeWebPush: {
        de: 'Benachrichtige mich per Push Notification',
        en: 'Notify me via Push Notification',
    },
    notificationsEmpty: {
        de: 'Keine Benachrichtigungen vorhanden',
        en: 'You have no notifications yet.',
    },
    okay: {
        de: 'Okay',
        en: 'Okay',
    },
    profile: {
        de: 'Profil',
        en: 'Profile',
    },
    save: {
        de: 'Speichern',
        en: 'Save',
    },
    savingFailed: {
        de: 'Speichern ist fehlgeschlagen',
        en: 'Saving has failed',
    },
    settings: {
        de: 'Einstellungen',
        en: 'Settings',
    },
    sms: {
        de: 'SMS',
        en: 'SMS',
    },
    subscribe: {
        de: 'Abbonnieren',
        en: 'Subscribe',
    },
    timezone: {
        de: 'Zeitzone',
        en: 'Timezone',
    },
    topics: {
        de: 'Themen',
        en: 'Topics',
    },
    unsubscribe: {
        de: 'Deabbonieren',
        en: 'Unsubscribe',
    },
    webpush: {
        de: 'Web Push',
        en: 'Web Push',
    },
    webpushConfirmText: {
        de: 'Notifications können jederzeit in den Browser Einstellungen deaktiviert werden.',
        en: 'Notifications can be turned off anytime from browser settings.',
    },
    webpushConfirmTitle: {
        de: 'Wir wollen dir Push Benachrichtigungen schenken',
        en: 'We want to send you push notifications.',
    },
    webpushTopics: {
        de: 'Abonniere die folgenden Themen um auf dem neuesten Stand zu sein. Du kannst dich jederzeit von diesen Themen abmelden oder auch später anmelden.',
        en: 'Subscribe to the following topics to be always up to date. You can unsubscribe any time you want or subscribe to these topics later.',
    },
};

const IS_DEV = global['window'] && (window.location.host.indexOf('localhost:3002') >= 0 || window.location.host.indexOf('localhost:5002') >= 0);

export function buildSDKConfig(opts: SDKConfig, scriptLocation: string | null | undefined) {
    const options: SDKConfig = <any>{ ...opts || {} };

    if (!isStringOption(options.apiUrl)) {
        logWarn('init.apiURL must be a string if defined, fallback to default.');
        options.apiUrl = undefined!;
    }

    if (!isString(options.apiUrl)) {
        options.apiUrl = buildBaseUrl(scriptLocation)!;
    }

    if (!isString(options.apiUrl)) {
        options.apiUrl = 'https://app.notifo.io';
    }

    while (options.apiUrl && options.apiUrl.endsWith('/')) {
        options.apiUrl = options.apiUrl.substring(0, options.apiUrl.length - 1);
    }

    if (!isStringOption(options.styleUrl)) {
        logWarn('init.styleUrl must be a string if defined, fallback to default.');
        options.styleUrl = undefined!;
    }

    if (!options.styleUrl && !IS_DEV) {
        options.styleUrl = `${options.apiUrl}/build/notifo-sdk.css`;
    }

    if (!isStringOption(options.serviceWorkerUrl)) {
        logWarn('init.serviceWorkerUrl must be a string if defined.');
        options.serviceWorkerUrl = undefined!;
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
        options.locale = undefined!;
    }

    if (!options.locale) {
        options.locale = 'en';
    }

    if (!isObject(options.allowedChannels)) {
        options.allowedChannels = { email: true, webpush: true };
    }

    if (isUndefined(options.allowProfile)) {
        options.allowProfile = true;
    }

    if (!isNumber(options.permissionDeniedLifetimeHours)) {
        options.permissionDeniedLifetimeHours = 7 * 24;
    }

    if (!options.apiKey && !options.userToken) {
        logWarn('init.apiKey or init.userToken must be defined');

        return null;
    }

    if (!isObject(options.texts)) {
        options.texts = {} as any;
    }

    for (const key of TextKeys) {
        if (!isString(options.texts[key]) || !options.texts[key]) {
            options.texts[key] = DefaultTexts[key][options.locale];
        }
    }

    return options;
}

function buildBaseUrl(url: string | null | undefined) {
    if (!isString(url)) {
        return null;
    }

    url = url.trim();

    let indexOfHash = url.indexOf('/', 'https://'.length);

    if (indexOfHash > 0) {
        url = url.substring(0, indexOfHash);
    }

    return url;
}

export function buildNotificationsOptions(opts: NotificationsOptions) {
    const options: NotificationsOptions = <any>{ ...opts || {} };

    if (!isEnumOption(options.position, SUPPORTED_POSITIONS)) {
        logWarn(`show-notifications.position is not one these values: ${SUPPORTED_POSITIONS.join(', ')}`);
        options.position = undefined!;
    }

    if (!options.position) {
        options.position = SUPPORTED_POSITIONS[0];
    }

    if (!isEnumOption(options.style, SUPPORTED_MAIN_STYLES)) {
        logWarn(`show-notifications.style must be one of these values: ${SUPPORTED_MAIN_STYLES.join(',')}`);
        options.style = undefined!;
    }

    if (!options.style) {
        options.style = SUPPORTED_MAIN_STYLES[0];
    }

    return options;
}

export function buildTopicOptions(opts: TopicOptions) {
    const options: TopicOptions = <any>{ ...opts || {} };

    if (!isEnumOption(options.position, SUPPORTED_POSITIONS)) {
        logWarn(`show-topic.position is not one these values: ${SUPPORTED_POSITIONS.join(', ')}`);
        options.position = undefined!;
    }

    if (!options.position) {
        options.position = SUPPORTED_POSITIONS[0];
    }

    if (!isEnumOption(options.style, SUPPORTED_TOPIC_STYLES)) {
        logWarn(`show-topic.style must be one of these values: ${SUPPORTED_TOPIC_STYLES.join(',')}`);
        options.style = undefined!;
    }

    if (!options.style) {
        options.style = SUPPORTED_TOPIC_STYLES[0];
    }

    return options;
}

function isStringOption(value: any) {
    return !value || isString(value);
}

function isEnumOption(value: any, allowed: ReadonlyArray<string>) {
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

    // The type of the api connection.
    connectionMode: ConnectionMode;

    // The timer interval.
    pollingInterval: number;

    // How long a deny of the web push permission will be valid. 
    permissionDeniedLifetimeHours: number;

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

    // An object of allowed channels.
    allowedChannels: {};

    // True when profile can be edited.
    allowProfile: boolean;

    // The url to the service worker.
    serviceWorkerUrl: string;

    // The locale settings for date formatting.
    locale: string;

    // All needed texts.
    texts: Texts<string>;

    // A callback that is invoked when a notification is retrieved.
    onNotification?: (notification: any) => void;
}

export interface SubscribeOptions {
    existingWorker?: true;
}

type OptionMainStyle = 'message' | 'chat' | 'chat_filled' | 'notifo';
type OptionPosition = 'bottom-left' | 'bottom-right';
type OptionTopicStyle = 'star' | 'heart' | 'alarm' | 'bell';

const SUPPORTED_MAIN_STYLES: ReadonlyArray<OptionMainStyle> = ['message', 'chat', 'chat_filled', 'notifo'];
const SUPPORTED_POSITIONS: ReadonlyArray<OptionPosition> = ['bottom-left', 'bottom-right'];
const SUPPORTED_TOPIC_STYLES: ReadonlyArray<OptionTopicStyle> = ['star', 'heart', 'bell', 'alarm'];

export type ConnectionMode = 'SignalR' | 'SignalRSockets' | 'Polling';

export interface TopicOptions {
    // The style of the button.
    style: OptionTopicStyle;

    // The position of the modal.
    position: OptionPosition;
}

export interface NotificationsOptions {
    // The position of the modal.
    position: OptionPosition;

    // The style of the button.
    style: OptionMainStyle;

    // True to hide the profile menu.
    hideProfile?: boolean;
}

type Texts<T> = {
    allow: T;
    archive: T;
    archiveLink: T;
    cancel: T;
    deny: T;
    email: T;
    emailAddress: T;
    fullName: T;
    language: T;
    loadingFailed: T;
    mobilepush: T;
    messaging: T;
    notifications: T;
    notificationsEmpty: T;
    notifyBeEmail: T;
    notifyBeWebPush: T;
    okay: T;
    profile: T;
    save: T;
    savingFailed: T;
    settings: T;
    sms: T;
    subscribe: T;
    timezone: T;
    topics: T;
    unsubscribe: T;
    webpush: T;
    webpushConfirmText: T;
    webpushConfirmTitle: T;
    webpushTopics: T;
};

const TextKeys: ReadonlyArray<keyof Texts<any>> = [
    'allow',
    'archive',
    'archiveLink',
    'cancel',
    'deny',
    'email',
    'emailAddress',
    'fullName',
    'language',
    'loadingFailed',
    'mobilepush',
    'messaging',
    'notifications',
    'notificationsEmpty',
    'notifyBeEmail',
    'notifyBeWebPush',
    'okay',
    'profile',
    'save',
    'savingFailed',
    'settings',
    'sms',
    'subscribe',
    'timezone',
    'topics',
    'unsubscribe',
    'webpush',
    'webpushConfirmText',
    'webpushConfirmTitle',
    'webpushTopics',
];
