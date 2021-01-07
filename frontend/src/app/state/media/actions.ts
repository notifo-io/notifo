/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { listThunk, Query } from '@app/framework';
import { Clients, MediaDto } from '@app/service';
import { createAction, createReducer } from '@reduxjs/toolkit';
import { Middleware } from 'redux';
import { createApiThunk, selectApp } from '../shared';
import { MediaState } from './state';

const list = listThunk<MediaState, MediaDto>('media', 'media', async params => {
    const { items, total } = await Clients.Media.getMedias(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const selectMedia = createAction<{ fileName: string }>('media/select');

export const loadMediaAsync = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const uploadMediaAsync = createApiThunk('media/upload',
    async (arg: { appId: string, file: File }) => {
        await Clients.Media.upload(arg.appId, arg.file);
    });

export const deleteMediaAsync = createApiThunk('media/delete',
    async (arg: { appId: string, fileName: string }) => {
        await Clients.Media.delete(arg.appId, arg.fileName);
    });

export const mediaMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (uploadMediaAsync.fulfilled.match(action) || deleteMediaAsync.fulfilled.match(action)) {
        const load: any = loadMediaAsync(action.meta.arg.appId);

        store.dispatch(load);
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
    .addCase(uploadMediaAsync.pending, (state, action) => {
        state.uploadingFiles.unshift(action.meta.arg.file);
    })
    .addCase(uploadMediaAsync.fulfilled, (state, action) => {
        state.uploadingFiles.remove(action.meta.arg.file);
    })
    .addCase(uploadMediaAsync.rejected, (state, action) => {
        state.uploadingFiles.remove(action.meta.arg.file);
    })
    .addCase(deleteMediaAsync.fulfilled, (state, action) => {
        state.uploadingFiles.removeBy(x => x.name, action.meta.arg.fileName);
    }));
