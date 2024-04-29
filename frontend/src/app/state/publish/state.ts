/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { MutationState } from '@app/framework';
import { PublishDto } from '@app/service';

export interface PublishStateInStore {
    publish: PublishState;
}

export interface PublishState {
    // The mutation for publishing.
    publishing?: MutationState;

    // True when the dialog is open.
    dialogOpen?: boolean;

    // The initial values.
    dialogValues?: Partial<PublishDto>;
}
