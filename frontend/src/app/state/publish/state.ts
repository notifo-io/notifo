/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo } from '@app/framework';
import { PublishDto } from '@app/service';

export interface PublishStateInStore {
    publish: PublishState;
}

export interface PublishState {
    // True when publishing.
    publishing?: boolean;

    // The publishing error.
    publishingError?: ErrorInfo;

    // True when the dialog is open.
    dialogOpen?: boolean;

    // The initial values.
    dialogValues?: Partial<PublishDto>;
}
