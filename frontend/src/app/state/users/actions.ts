/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { addOrModify, buildError, List, Query, remove } from '@app/framework';
import { Clients, UpsertUserDto, UserDto } from '@app/service';
import { Dispatch, Middleware, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { UsersState } from './state';

export const USER_UPSERT_STARTED = 'USER_UPSERT_STARTED';
export const USER_UPSERT_FAILED = 'USER_UPSERT_FAILED';
export const USER_UPSERT_SUCCEEEDED = 'USER_UPSERT_SUCCEEEDED';
export const USER_DELETE_STARTED = 'USER_DELETE_STARTED';
export const USER_DELETE_FAILED = 'USER_DELETE_FAILED';
export const USER_DELETE_SUCCEEEDED = 'USER_DELETE_SUCCEEEDED';
export const USER_LOAD_STARTED = 'USER_LOAD_STARTED';
export const USER_LOAD_FAILED = 'USER_LOAD_FAILED';
export const USER_LOAD_SUCCEEEDED = 'USER_LOAD_SUCCEEEDED';

const list = new List<UserDto>('users', 'users', async (params) => {
    const { items, total } = await Clients.Users.getUsers(params.appId, params.search, params.pageSize, params.page * params.pageSize);

    return { items, total };
});

export const loadUsersAsync = (appId: string, q?: Partial<Query>, reset = false) => {
    return list.load(q, { appId }, reset);
};

export const loadUserAsync = (appId: string, userId: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: USER_LOAD_STARTED });

        try {
            const user = await Clients.Users.getUser(appId, userId);

            dispatch({ type: USER_LOAD_SUCCEEEDED, user });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: USER_LOAD_FAILED, error });
        }
    };
};

export const upsertUserAsync = (appId: string, params: UpsertUserDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: USER_UPSERT_STARTED });

        try {
            const response = await Clients.Users.postUsers(appId, { requests: [params] });

            const user = response[0];

            dispatch({ type: USER_UPSERT_SUCCEEEDED, appId, user });

        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: USER_UPSERT_FAILED, error });
        }
    };
};

export const deleteUserAsync = (appId: string, id: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: USER_DELETE_FAILED });

        try {
            await Clients.Users.deleteUser(appId, id);

            dispatch({ type: USER_DELETE_SUCCEEEDED, appId, id });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: USER_UPSERT_FAILED, error });
        }
    };
};

export function usersMiddleware(): Middleware {
    const middleware: Middleware = store => next => action => {
        const result = next(action);

        if (action.type === USER_UPSERT_SUCCEEEDED) {
            const load: any = loadUsersAsync(action.appId);

            store.dispatch(load);
        }

        return result;
    };

    return middleware;
}

export function usersReducer(): Reducer<UsersState> {
    const initialState: UsersState = {
        users: list.createInitial(),
    };

    const reducer: Reducer<UsersState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            case USER_LOAD_STARTED:
                return {
                    ...state,
                    loadingUser: true,
                    loadingUserError: null,
                };
            case USER_LOAD_FAILED:
                return {
                    ...state,
                    loadingUser: false,
                    loadingUserError: action.error,
                };
            case USER_LOAD_SUCCEEEDED:
                return {
                    ...state,
                    loadingUser: false,
                    loadingUserError: null,
                    user: action.user,
                };
            case USER_UPSERT_STARTED:
                return {
                    ...state,
                    upserting: true,
                    upsertingError: null,
                };
            case USER_UPSERT_FAILED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: action.error,
                };
            case USER_UPSERT_SUCCEEEDED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: null,
                    user: state.user?.id === action.user.id ? action.user : state.user,
                    users: list.changeItems(state, items => addOrModify(items, action.user, 'id')),
                };
            case USER_DELETE_SUCCEEEDED:
                return {
                    ...state,
                    users: list.changeItems(state, items => remove(items, action.id, 'id')),
                };
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
