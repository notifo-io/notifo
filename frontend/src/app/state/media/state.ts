/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { MediaDto } from '@app/service/service';

export interface MediaStateInStore {
    media: MediaState;
}

export interface MediaState {
    // All media items.
    media: ListState<MediaDto>;

    // The uploading files.
    uploadingFiles: ReadonlyArray<File>;

    // The mutation for uploading media files.
    uploading?: MutationState;

    // The mutation for deleting media files.
    deleting?: MutationState;
}
