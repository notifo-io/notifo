/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { toast } from 'react-toastify';
import { Middleware } from 'redux';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients } from '@app/service';
import { selectApp } from './../shared';
import { MediaState, MediaStateInStore } from './state';

export const loadMedia = createList<MediaState, MediaStateInStore>('media', 'media').with({
    name: 'media/load',
    queryFn: async (p: { appId: string }, q) => {
        const { items, total } = await Clients.Media.getMedias(p.appId, q.search, q.take, q.skip);

        return { items, total };
    },
});

export const uploadMedia = createMutation<MediaState>('uploading').with({
    name: 'media/upload',
    mutateFn: async (arg: { appId: string; file: File }) => {
        await Clients.Media.upload(arg.appId, { data: arg.file, fileName: arg.file.name });
    },
});

export const deleteMedia = createMutation<MediaState>('deleting').with({
    name: 'media/delete',
    mutateFn: async (arg: { appId: string; fileName: string }) => {
        await Clients.Media.delete(arg.appId, arg.fileName);
    },
    updateFn(state, action) {
        state.uploadingFiles.removeByValue(x => x.name, action.meta.arg.fileName);
    },
});

export const mediaMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (uploadMedia.fulfilled.match(action) || deleteMedia.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadMedia({ appId }) as any);
    } else if (deleteMedia.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: MediaState = {
    media: loadMedia.createInitial(), uploadingFiles: [],
};

const operations = [
    deleteMedia,
    loadMedia,
    uploadMedia,
];

export const mediaReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }),
operations);
