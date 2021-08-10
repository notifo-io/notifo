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
    'webhook',
    'webpush',
];

export const CONFIRM_MODES = [{
    value: 'Seen',
    label: texts.common.confirmModes.seen,
}, {
    value: 'Explicit',
    label: texts.common.confirmModes.explicit,
}, {
    value: 'None',
    label: texts.common.confirmModes.none,
},
];

export const SEND_MODES = [{
    value: 'Inherit',
    label: texts.common.sendModes.inherit,
}, {
    value: 'Send',
    label: texts.common.sendModes.send,
}, {
    value: 'DoNotSend',
    label: texts.common.sendModes.doNotSend,
}, {
    value: 'DoNotAllow',
    label: texts.common.sendModes.doNotAllow,
},
];
