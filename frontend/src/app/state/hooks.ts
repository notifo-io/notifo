/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { useSelector } from 'react-redux';
import { AppsState, AppsStateInStore } from './apps';
import { CoreState, CoreStateInStore } from './core';
import { EmailTemplatesState, EmailTemplatesStateInStore } from './email-templates';
import { EventsState, EventsStateInStore } from './events';
import { IntegrationsState, IntegrationsStateInStore } from './integrations';
import { LogState, LogStateInStore } from './log';
import { LoginState, LoginStateInStore } from './login';
import { MediaState, MediaStateInStore } from './media';
import { MessagingTemplatesState, MessagingTemplatesStateInStore } from './messaging-templates';
import { NotificationsState, NotificationsStateInStore } from './notifications';
import { PublishState, PublishStateInStore } from './publish';
import { SmsTemplatesState, SmsTemplatesStateInStore } from './sms-templates';
import { SubscriptionsState, SubscriptionsStateInStore } from './subscriptions';
import { TemplatesState, TemplatesStateInStore } from './templates';
import { UsersState, UsersStateInStore } from './users';

type State =
    AppsStateInStore &
    CoreStateInStore &
    EmailTemplatesStateInStore &
    EventsStateInStore &
    IntegrationsStateInStore &
    LogStateInStore &
    LoginStateInStore &
    MediaStateInStore &
    MessagingTemplatesStateInStore &
    NotificationsStateInStore &
    PublishStateInStore &
    SmsTemplatesStateInStore &
    SubscriptionsStateInStore &
    TemplatesStateInStore &
    UsersStateInStore;

export function useStore<T>(mapping: (state: State) => T) {
    return useSelector<State, T>(mapping);
}

export function useApps<T>(mapping: (state: AppsState) => T) {
    return useStore<T>(x => mapping(x.apps));
}

export function useCore<T>(mapping: (state: CoreState) => T) {
    return useStore<T>(x => mapping(x.core));
}

export function useEmailTemplates<T>(mapping: (state: EmailTemplatesState) => T) {
    return useStore<T>(x => mapping(x.emailTemplates));
}

export function useEvents<T>(mapping: (state: EventsState) => T) {
    return useStore<T>(x => mapping(x.events));
}

export function useIntegrations<T>(mapping: (state: IntegrationsState) => T) {
    return useStore<T>(x => mapping(x.integrations));
}

export function useLog<T>(mapping: (state: LogState) => T) {
    return useStore<T>(x => mapping(x.log));
}

export function useLogin<T>(mapping: (state: LoginState) => T) {
    return useStore<T>(x => mapping(x.login));
}

export function useMedia<T>(mapping: (state: MediaState) => T) {
    return useStore<T>(x => mapping(x.media));
}

export function useMessagingTemplates<T>(mapping: (state: MessagingTemplatesState) => T) {
    return useStore<T>(x => mapping(x.messagingTemplates));
}

export function useNotifications<T>(mapping: (state: NotificationsState) => T) {
    return useStore<T>(x => mapping(x.notifications));
}

export function usePublish<T>(mapping: (state: PublishState) => T) {
    return useStore<T>(x => mapping(x.publish));
}

export function useSmsTemplates<T>(mapping: (state: SmsTemplatesState) => T) {
    return useStore<T>(x => mapping(x.smsTemplates));
}

export function useSubscriptions<T>(mapping: (state: SubscriptionsState) => T) {
    return useStore<T>(x => mapping(x.subscriptions));
}

export function useTemplates<T>(mapping: (state: TemplatesState) => T) {
    return useStore<T>(x => mapping(x.templates));
}

export function useUsers<T>(mapping: (state: UsersState) => T) {
    return useStore<T>(x => mapping(x.users));
}
