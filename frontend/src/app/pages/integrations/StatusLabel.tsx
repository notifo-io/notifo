/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Badge } from 'reactstrap';
import { IntegrationStatus } from '@app/service';
import { texts } from '@app/texts';

export const StatusLabel = (props: { status: IntegrationStatus }) => {
    const [color, text] = getStatusText(props.status);

    return (
        <Badge pill color={color}>{text}</Badge>
    );
};

function getStatusText(status: IntegrationStatus) {
    switch (status) {
        case 'Pending':
            return ['warning', texts.common.pending];
        case 'Verified':
            return ['success', texts.common.verified];
        case 'VerificationFailed':
            return ['danger', texts.common.failed];
        default:
            return ['default', texts.common.notStarted];
    }
}
