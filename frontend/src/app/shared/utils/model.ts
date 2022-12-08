/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ChannelCondition, ChannelRequired, ChannelSend, ChannelSettingDto, ConfirmMode, TopicChannel } from '@app/service';
import { texts } from '@app/texts';

export const CHANNELS = [
    'email',
    'messaging',
    'mobilepush',
    'sms',
    'webpush',
    'webhook',
];

export function fillChannelSettings<T>(source: T, property: keyof T) {
    const settings: { [key: string]: ChannelSettingDto } = (source as any)[property] || {};

    for (const channel of CHANNELS) {
        settings[channel] ||= { send: 'Inherit', condition: 'Inherit', required: 'Inherit' };
    }

    (source as any)[property] = settings;

    return source;
}

type Mode<T> = { value: T | string; label: string };

export const CONFIRM_MODES: ReadonlyArray<Mode<ConfirmMode>> = [{
    value: 'Explicit',
    label: texts.common.confirmModes.Explicit,
}, {
    value: 'None',
    label: texts.common.confirmModes.None,
}];

export const SEND_MODES: ReadonlyArray<Mode<ChannelSend>> = [{
    value: 'Inherit',
    label: texts.common.sendModes.Inherit,
}, {
    value: 'Send',
    label: texts.common.sendModes.Send,
}, {
    value: 'NotSending',
    label: texts.common.sendModes.NotSending,
}, {
    value: 'NotAllowed',
    label: texts.common.sendModes.NotAllowed,
}];

export const REQUIRED_MODES: ReadonlyArray<Mode<ChannelRequired>> = [{
    value: 'Inherit',
    label: texts.common.requiredModes.Inherit,
}, {
    value: 'Required',
    label: texts.common.requiredModes.Required,
}, {
    value: 'NotRequired',
    label: texts.common.requiredModes.NotRequired,
}];

export const CONDITION_MODES: ReadonlyArray<Mode<ChannelCondition>> = [{
    value: 'Inherit',
    label: texts.common.conditionModes.Inherit,
}, {
    value: 'Always',
    label: texts.common.conditionModes.Always,
}, {
    value: 'IfNotSeen',
    label: texts.common.conditionModes.IfNotSeen,
}, {
    value: 'IfNotConfirmed',
    label: texts.common.conditionModes.IfNotConfirmed,
}];

export const ALLOWED_MODES: ReadonlyArray<Mode<TopicChannel>> = [{
    value: 'Allowed',
    label: texts.common.allowedModes.Allowed,
}, {
    value: 'NotAllowed',
    label: texts.common.allowedModes.NotAllowed,
}];