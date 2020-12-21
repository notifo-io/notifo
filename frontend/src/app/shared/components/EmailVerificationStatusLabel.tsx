/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { EmailVerificationStatus } from '@app/service';
import { texts } from '@app/texts';
import * as React from 'react';
import { Badge } from 'reactstrap';

export const EmailVerificationStatusLabel = (props: { status: EmailVerificationStatus }) => {
    const [color, text] = getStatusText(props.status);

    return (
        <div className='settings-email-verification'>
            <span>{texts.common.emailVerificationStatus}</span> <Badge pill color={color}>{text}</Badge>
        </div>
    );
};

function getStatusText(status: EmailVerificationStatus) {
    switch (status) {
        case 'Pending':
            return ['warning', texts.common.pending];
        case 'Verified':
            return ['success', texts.common.verified];
        case 'Failed':
            return ['danger', texts.common.failed];
        default:
            return ['default', texts.common.notStarted];
    }
}
