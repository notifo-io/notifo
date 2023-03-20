/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { Middleware } from 'redux';
import { formatError, listThunk, Query } from '@app/framework';
import { Clients, MediaDto } from '@app/service';
import { createApiThunk, selectApp } from '../shared';
import { MediaState } from './state';

const list = listThunk<MediaState, MediaDto>('media', 'media', async params => {
    const { items, total } = await Clients.Media.getMedias(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadMedia = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const uploadMedia = createApiThunk('media/upload',
    async (arg: { appId: string; file: File }) => {
        await Clients.Media.upload(arg.appId, { data: arg.file, fileName: arg.file.name });
    });

export const deleteMedia = createApiThunk('media/delete',
    async (arg: { appId: string; fileName: string }) => {
        await Clients.Media.delete(arg.appId, arg.fileName);
    });

export const mediaMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (uploadMedia.fulfilled.match(action) || deleteMedia.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadMedia(appId) as any);
    } else if (deleteMedia.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: MediaState = {
    media: list.createInitial(), uploadingFiles: [],
};

export const mediaReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(deleteMedia.fulfilled, (state, action) => {
        state.uploadingFiles.removeByValue(x => x.name, action.meta.arg.fileName);
    }));
