/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { de, enUS, tr } from 'date-fns/locale';
import { isFunction, isNumber, isObject, isString, isUndefined, logWarn } from './utils';

export const SUPPORTED_LOCALES = {
    en: enUS,
    de,
    tr,
};

const DefaultTexts: Texts<{ de: string; en: string; tr: string }> = {
    allow: {
        de: 'Erlauben',
        en: 'Allow',
        tr: 'İzin ver',
    },
    archive: {
        de: 'Archiv',
        en: 'Archive',
        tr: 'Arşiv',
    },
    archiveLink: {
        de: 'Gelöschte Benachrichtungen',
        en: 'Deleted notifications',
        tr: 'Silinmiş bildirimler',
    },
    cancel: {
        de: 'Abbrechen',
        en: 'Cancel',
        tr: 'Vazgeç',
    },
    deny: {
        de: 'Nein, Danke',
        en: 'Deny',
        tr: 'Engelle',
    },
    email: {
        de: 'E-Mail',
        en: 'E-Mail',
        tr: 'E-Posta',
    },
    emailAddress: {
        de: 'E-Mail Adresse',
        en: 'E-Mail Address',
        tr: 'E-posta adresi',
    },
    fullName: {
        de: 'Name',
        en: 'Name',
        tr: 'İsim',
    },
    language: {
        de: 'Sprache',
        en: 'Language',
        tr: 'Dil',
    },
    loadingFailed: {
        de: 'Laden ist fehlgeschlagen',
        en: 'Loading has failed',
        tr: 'Giriş başarısız',
    },
    mobilepush: {
        de: 'Mobile Push',
        en: 'Mobile Push',
        tr: 'Mobil Bildirim',
    },
    messaging: {
        de: 'Messaging',
        en: 'Messaging',
        tr: 'Mesajlaşma',
    },
    notifications: {
        de: 'Nachrichten',
        en: 'Notifications',
        tr: 'Bildirimler',
    },
    notifyBeEmail: {
        de: 'Benachrichtige mich per Email',
        en: 'Notify me via Email',
        tr: 'Beni E-posta ile bilgilendir',
    },
    notifyBeWebPush: {
        de: 'Benachrichtige mich per Push Notification',
        en: 'Notify me via Push Notification',
        tr: 'Beni anlık bildirim ile bilgilendir',
    },
    notificationsEmpty: {
        de: 'Keine Benachrichtigungen vorhanden',
        en: 'You have no notifications yet.',
        tr: 'Henüz hiç bildirim almadınız',
    },
    okay: {
        de: 'Okay',
        en: 'Okay',
        tr: 'Tamam',
    },
    profile: {
        de: 'Profil',
        en: 'Profile',
        tr: 'Profil',
    },
    save: {
        de: 'Speichern',
        en: 'Save',
        tr: 'Kaydet',
    },
    savingFailed: {
        de: 'Speichern ist fehlgeschlagen',
        en: 'Saving has failed',
        tr: 'Kaydedilemedi',
    },
    settings: {
        de: 'Einstellungen',
        en: 'Settings',
        tr: 'Ayarlar',
    },
    sms: {
        de: 'SMS',
        en: 'SMS',
        tr: 'SMS',
    },
    subscribe: {
        de: 'Abbonnieren',
        en: 'Subscribe',
        tr: 'Abone Ol',
    },
    timezone: {
        de: 'Zeitzone',
        en: 'Timezone',
        tr: 'Zaman dilimi',
    },
    topics: {
        de: 'Themen',
        en: 'Topics',
        tr: 'Başlıklar',
    },
    unsubscribe: {
        de: 'Deabbonieren',
        en: 'Unsubscribe',
        tr: 'Abonelikten ayrıl',
    },
    webpush: {
        de: 'Web Push',
        en: 'Web Push',
        tr: 'Web Anlık Bildirimi',
    },
    webpushConfirmText: {
        de: 'Notifications können jederzeit in den Browser Einstellungen deaktiviert werden.',
        en: 'Notifications can be turned off anytime from browser settings.',
        tr: 'Bildirimler tarayıcı ayarlarından istediğiniz zaman kapatılabilir.',
    },
    webpushConfirmTitle: {
        de: 'Wir wollen dir Push Benachrichtigungen schenken',
        en: 'We want to send you push notifications.',
        tr: 'Size anlık bildirimleri gönderebilmek istiyoruz.',
    },
    webpushTopics: {
        de: 'Abonniere die folgenden Themen um auf dem neuesten Stand zu sein. Du kannst dich jederzeit von diesen Themen abmelden oder auch später anmelden.',
        en: 'Subscribe to the following topics to be always up to date. You can unsubscribe any time you want or subscribe to these topics later.',
        tr: 'Her zaman güncel kalmak için aşağıdaki başlıklara abone olunuz. İstediğiniz zaman abonelikten ayrılabilir veya daha sonra abone olabilirsiniz.',
    },
};

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

    if (!isStringOption(options.serviceWorkerUrl)) {
        logWarn('init.serviceWorkerUrl must be a string if defined.');
        options.serviceWorkerUrl = undefined!;
    }

    if (!options.serviceWorkerUrl) {
        options.serviceWorkerUrl = '/notifo-sw.js';
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

    if (!isFunctionOption(options.onNotification)) {
        logWarn('init.onNotification must be a function if defined.');
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
            options.texts[key] = (DefaultTexts as any)[key][options.locale];
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
        options.position = 'bottom-right';
    }

    if (!isEnumOption(options.style, SUPPORTED_MAIN_STYLES)) {
        logWarn(`show-notifications.style must be one of these values: ${SUPPORTED_MAIN_STYLES.join(',')}`);
        options.style = undefined!;
    }

    if (!options.style) {
        options.style = 'message';
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
        options.position = 'bottom-left';
    }

    if (!isEnumOption(options.style, SUPPORTED_TOPIC_STYLES)) {
        logWarn(`show-topic.style must be one of these values: ${SUPPORTED_TOPIC_STYLES.join(',')}`);
        options.style = undefined!;
    }

    if (!options.style) {
        options.style = 'star';
    }

    return options;
}

function isStringOption(value: any) {
    return !value || isString(value);
}

function isFunctionOption(value: any) {
    return !value || isFunction(value);
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

    // Allow overriding the link target.
    linkTarget?: string;

    // An object of allowed channels.
    allowedChannels: Record<string, boolean>;

    // True when profile can be edited.
    allowProfile: boolean;

    // The url to the service worker.
    serviceWorkerUrl: string;

    // The locale settings for date formatting.
    locale: string;

    // All needed texts.
    texts: Texts<string>;

    // Shown when the notification is confirmed.
    onConfirm?: (notification: any) => void;

    // A callback that is invoked when a notification is retrieved.
    onNotification?: (notification: any) => void;
}

export interface SubscribeOptions {
    existingWorker?: true;

    // The render function.
    onSubscribeDialog?: (config: SDKConfig, allow: () => void, deny: () => void) => void;
}

type OptionMainStyle = 'bell' | 'bell_filled' | 'chat' | 'chat_filled' | 'message' | 'notifo';
type OptionPosition = 'bottom-left' | 'bottom-right';
type OptionTopicStyle = 'alarm' | 'bell' | 'heart' | 'star';

const SUPPORTED_MAIN_STYLES: ReadonlyArray<OptionMainStyle> = ['bell', 'bell_filled', 'chat', 'chat_filled', 'message', 'notifo'];
const SUPPORTED_POSITIONS: ReadonlyArray<OptionPosition> = ['bottom-left', 'bottom-right'];
const SUPPORTED_TOPIC_STYLES: ReadonlyArray<OptionTopicStyle> = ['alarm', 'bell', 'heart', 'star'];

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

    [key: string]: T;
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
