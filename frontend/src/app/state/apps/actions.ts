/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { addOrModify, buildError, List } from '@app/framework';
import { AddContributorDto, AppDto, Clients, UpsertAppDto } from '@app/service';
import { Dispatch, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { AppsState, CreateAppParams } from './state';

export const APP_CREATE_RESET = 'APP_CREATE_RESET';
export const APP_LOAD_DETAILS_STARTED = 'APP_LOAD_DETAILS_STARTED';
export const APP_LOAD_DETAILS_FAILED = 'APP_LOAD_DETAILS_FAILED';
export const APP_LOAD_DETAILS_SUCCEEEDED = 'APP_LOAD_DETAILS_SUCCEEEDED';
export const APP_CREATE_STARTED = 'APP_CREATE_STARTED';
export const APP_CREATE_FAILED = 'APP_CREATE_FAILED';
export const APP_CREATE_SUCCEEEDED = 'APP_CREATE_SUCCEEEDED';
export const APP_UPSERT_STARTED = 'APP_UPSERT_STARTED';
export const APP_UPSERT_FAILED = 'APP_UPSERT_FAILED';
export const APP_UPSERT_SUCCEEEDED = 'APP_UPSERT_SUCCEEEDED';
export const APP_UPSERT_CONTRIBUTORS_STARTED = 'APP_UPSERT_CONTRIBUTORS_STARTED';
export const APP_UPSERT_CONTRIBUTORS_FAILED = 'APP_UPSERT_CONTRIBUTORS_FAILED';
export const APP_UPSERT_CONTRIBUTORS_SUCCEEEDED = 'APP_UPSERT_CONTRIBUTORS_SUCCEEEDED';

const list = new List<AppDto>('apps', 'apps', async () => {
    const items = await Clients.Apps.getApps();

    return { items, total: items.length };
});

export const selectApp = (appId: string) => {
    return { type: APP_SELECTED, appId };
};

export const resetCreateApp = () => {
    return { type: APP_CREATE_RESET };
};

export const loadAppsAsync = () => {
    return list.load();
};

export const loadDetailsAsync = (appId: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: APP_LOAD_DETAILS_STARTED });

        try {
            const app = await Clients.Apps.getApp(appId);

            dispatch({ type: APP_LOAD_DETAILS_SUCCEEEDED, app });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: APP_LOAD_DETAILS_FAILED, error });
        }
    };
};

export const addContributorAsync = (appId: string, params: AddContributorDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: APP_UPSERT_CONTRIBUTORS_STARTED });

        try {
            const app = await Clients.Apps.postContributor(appId, params);

            dispatch({ type: APP_UPSERT_CONTRIBUTORS_SUCCEEEDED, app });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: APP_UPSERT_CONTRIBUTORS_FAILED, error });
        }
    };
};

export const removeContributorAsync = (appId: string, id: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: APP_UPSERT_CONTRIBUTORS_STARTED });

        try {
            const app = await Clients.Apps.deleteContributor(appId, id);

            dispatch({ type: APP_UPSERT_CONTRIBUTORS_SUCCEEEDED, app });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: APP_UPSERT_CONTRIBUTORS_FAILED, error });
        }
    };
};

export const createAppAsync = (params: CreateAppParams) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: APP_CREATE_STARTED });

        try {
            const app = await Clients.Apps.postApp(params);

            dispatch({ type: APP_CREATE_SUCCEEEDED, app });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: APP_CREATE_FAILED, error });
        }
    };
};

export const upsertAppAsync = (appId: string, params: UpsertAppDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: APP_UPSERT_STARTED });

        try {
            const app = await Clients.Apps.putApp(appId, params);

            dispatch({ type: APP_UPSERT_SUCCEEEDED, app });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: APP_UPSERT_FAILED, error });
        }
    };
};

export function appsReducer(): Reducer<AppsState> {
    const initialState: AppsState = {
        apps: list.createInitial(),
    };

    const reducer: Reducer<AppsState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_CREATE_RESET:
                return {
                    ...state,
                    creating: false,
                    creatingError: null,
                };
            case APP_LOAD_DETAILS_STARTED:
                return {
                    ...state,
                    contributorsError: null,
                    contributorsUpdating: false,
                    loadingDetails: true,
                    loadingDetailsError: null,
                };
            case APP_LOAD_DETAILS_FAILED:
                return {
                    ...state,
                    loadingDetails: false,
                    loadingDetailsError: action.error,
                };
            case APP_LOAD_DETAILS_SUCCEEEDED:
                return {
                    ...state,
                    loadingDetails: false,
                    loadingDetailsError: null,
                    appDetails: action.app,
                };
            case APP_UPSERT_CONTRIBUTORS_STARTED:
                return {
                    ...state,
                    contributorsUpdating: true,
                    contributorsError: null,
                };
            case APP_UPSERT_CONTRIBUTORS_FAILED:
                return {
                    ...state,
                    contributorsUpdating: false,
                    contributorsError: action.error,
                };
            case APP_UPSERT_CONTRIBUTORS_SUCCEEEDED:
                return {
                    ...state,
                    contributorsUpdating: false,
                    contributorsError: null,
                    appDetails: action.app,
                };
            case APP_CREATE_STARTED:
                return {
                    ...state,
                    creating: true,
                    creatingError: null,
                };
            case APP_CREATE_FAILED:
                return {
                    ...state,
                    creating: false,
                    creatingError: action.error,
                };
            case APP_CREATE_SUCCEEEDED:
                return {
                    ...state,
                    creating: false,
                    creatingError: null,
                    apps: list.changeItems(state, items => addOrModify(items, action.app, 'id')),
                };
            case APP_UPSERT_STARTED:
                return {
                    ...state,
                    upserting: true,
                    upsertingError: null,
                };
            case APP_UPSERT_FAILED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: action.error,
                };
            case APP_UPSERT_SUCCEEEDED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: null,
                    apps: list.changeItems(state, items => addOrModify(items, action.app, 'id')),
                    appDetails: action.app,
                };
            case APP_SELECTED:
                return {
                    ...state,
                    appId: action.appId,
                };
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
