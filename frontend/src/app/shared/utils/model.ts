/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';

export const CHANNELS = [
    'email',
    'messaging',
    'mobilepush',
    'sms',
    'webpush',
    'webhook',
];

export const CONFIRM_MODES = [{
    value: 'Explicit',
    label: texts.common.confirmModes.explicit,
}, {
    value: 'None',
    label: texts.common.confirmModes.none,
}];

export const SEND_MODES = [{
    value: 'Inherit',
    label: texts.common.sendModes.inherit,
}, {
    value: 'Send',
    label: texts.common.sendModes.send,
}, {
    value: 'NotSending',
    label: texts.common.sendModes.doNotSend,
}, {
    value: 'NotAllowed',
    label: texts.common.sendModes.doNotAllow,
}];

export const CONDITION_MODES = [{
    value: 'Inherit',
    label: texts.common.conditionModes.inherit,
}, {
    value: 'Always',
    label: texts.common.conditionModes.always,
}, {
    value: 'IfNotSeen',
    label: texts.common.conditionModes.ifNotSeen,
}, {
    value: 'IfNotConfirmed',
    label: texts.common.conditionModes.ifNotConfirmed,
}];

export const ALLOWED_MODES = [{
    value: 'Allowed',
    label: texts.common.allowed,
}, {
    value: 'NotAllowed',
    label: texts.common.notAllowed,
}];