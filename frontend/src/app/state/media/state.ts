/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState } from '@app/framework';
import { MediaDto } from '@app/service/service';

export interface MediaStateInStore {
    media: MediaState;
}

export interface MediaState {
    // All media items.
    media: ListState<MediaDto>;

    // The uploading files.
    uploadingFiles: ReadonlyArray<File>;
}
