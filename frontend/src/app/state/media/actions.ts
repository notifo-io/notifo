/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError, List, Query, remove } from '@app/framework';
import { Clients, MediaDto } from '@app/service';
import { Dispatch, Middleware, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { MediaState } from './state';

export const MEDIAS_SELECT = 'MEDIAS_SELECT';
export const MEDIA_UPLOAD_STARTED = 'MEDIA_UPLOAD_STARTED';
export const MEDIA_UPLOAD_FAILED = 'MEDIA_UPLOAD_FAILED';
export const MEDIA_UPLOAD_SUCCEEEDED = 'MEDIA_UPLOAD_SUCCEEEDED';
export const MEDIA_DELETE_STARTED = 'MEDIA_DELETE_STARTED';
export const MEDIA_DELETE_FAILED = 'MEDIA_DELETE_FAILED';
export const MEDIA_DELETE_SUCCEEEDED = 'MEDIA_DELETE_SUCCEEEDED';

const list = new List<MediaDto>('media', 'media', async (params) => {
    const { items, total } = await Clients.Media.getMedias(params.appId, params.search, params.pageSize, params.page * params.pageSize);

    return { items, total };
});

export const selectMedia = (mediaCode?: string) => {
    return { type: MEDIAS_SELECT, mediaCode };
};

export const loadMediaAsync = (appId: string, q?: Partial<Query>, reset = false) => {
    return list.load(q, { appId }, reset);
};

export const uploadMediaAsync = (appId: string, file: File) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: MEDIA_UPLOAD_STARTED, file, appId });

        try {
            await Clients.Media.upload(appId, file);

            dispatch({ type: MEDIA_DELETE_SUCCEEEDED, file, appId });
        } catch (error) {
            dispatch({ type: MEDIA_DELETE_FAILED, error, file, appId });
        }
    };
};

export function mediaMiddleware(): Middleware {
    const middleware: Middleware = store => next => action => {
        const result = next(action);

        if (action.type === MEDIA_UPLOAD_SUCCEEEDED || action.type === MEDIA_DELETE_SUCCEEEDED) {
            const load: any = loadMediaAsync(action.appId);

            store.dispatch(load);
        }

        return result;
    };

    return middleware;
}

export const deleteMediaAsync = (appId: string, fileName: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: MEDIA_DELETE_FAILED });

        try {
            await Clients.Media.delete(appId, fileName);

            dispatch({ type: MEDIA_DELETE_SUCCEEEDED, appId, fileName });

        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: MEDIA_DELETE_FAILED, error });
        }
    };
};

export function mediaReducer(): Reducer<MediaState> {
    const initialState: MediaState = {
        media: list.createInitial(),
        uploadingFiles: [],
    };

    const reducer: Reducer<MediaState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            case MEDIAS_SELECT:
                return {
                    ...state,
                    currentMediaCode: action.mediaCode,
                };
            case MEDIA_UPLOAD_STARTED:
                return {
                    ...state,
                    uploadingFiles: [action.file, ...state.uploadingFiles],
                };
            case MEDIA_UPLOAD_FAILED:
                return {
                    ...state,
                    uploadingFiles: state.uploadingFiles.filter(x => x !== action.file),
                };
            case MEDIA_UPLOAD_SUCCEEEDED: {
                return {
                    ...state,
                    uploadingFiles: state.uploadingFiles.filter(x => x !== action.file),
                };
            }
            case MEDIA_DELETE_SUCCEEEDED: {
                return {
                    ...state,
                    media: list.changeItems(state, items => remove(items, action.fileName, 'fileName')),
                };
            }
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
