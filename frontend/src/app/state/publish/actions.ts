/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError } from '@app/framework';
import { Clients, PublishRequestDto } from '@app/service';
import { Dispatch, Reducer } from 'redux';
import { PublishState } from './state';

export const PUBLISH_PUBLISH_STARTED = 'PUBLISH_PUBLISH_STARTED';
export const PUBLISH_PUBLISH_SUCCEEDED = 'PUBLISH_PUBLISH_SUCCEEDED';
export const PUBLISH_PUBLISH_FAILED = 'PUBLISH_PUBLISH_FAILED';
export const PUBLISH_DIALOG_SHOW = 'PUBLISH_DIALOG_SHOW';
export const PUBLISH_DIALOG_HIDE = 'PUBLISH_DIALOG_HIDE';

export const openPublishDialog = (params?: Partial<PublishRequestDto>) => {
    return { type: PUBLISH_DIALOG_SHOW, params };
};

export const hidePublishDialog = () => {
    return { type: PUBLISH_DIALOG_HIDE };
};

export const publishAsync = (appId: string, params: PublishRequestDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: PUBLISH_PUBLISH_STARTED });

        try {
            await Clients.Events.postEvents(appId, { requests: [params] });

            dispatch({ type: PUBLISH_PUBLISH_SUCCEEDED });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: PUBLISH_PUBLISH_FAILED, error });
        }
    };
};

export function publishReducer(): Reducer<PublishState> {
    const initialState: PublishState = {};

    const reducer: Reducer<PublishState> = (state = initialState, action) => {
        switch (action.type) {
            case PUBLISH_DIALOG_SHOW:
                return {
                    ...state,
                    dialogOpen: true,
                    dialogValues: action.params,
                };
            case PUBLISH_DIALOG_HIDE:
                return {
                    ...state,
                    dialogOpen: false,
                    dialogValues: undefined,
                };
            case PUBLISH_PUBLISH_STARTED:
                return {
                    ...state,
                    publishing: true,
                    publishingError: null,
                };
            case PUBLISH_PUBLISH_FAILED:
                return {
                    ...state,
                    publishing: false,
                    publishingError: action.error,
                };
            case PUBLISH_PUBLISH_SUCCEEDED:
                return {
                    ...state,
                    publishing: false,
                    publishingError: null,
                };
            default:
                return state;
        }
    };

    return reducer;
}
