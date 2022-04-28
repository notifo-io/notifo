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
    label: texts.common.confirmModes.Explicit,
}, {
    value: 'None',
    label: texts.common.confirmModes.None,
}];

export const SEND_MODES = [{
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

export const CONDITION_MODES = [{
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

export const ALLOWED_MODES = [{
    value: 'Allowed',
    label: texts.common.allowedModes.Allowed,
}, {
    value: 'NotAllowed',
    label: texts.common.allowedModes.NotAllowed,
}];