/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { Middleware } from 'redux';
import { ErrorDto, listThunk, Query } from '@app/framework';
import { Clients, UpsertUserDto, UserDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { UsersState } from './state';

const list = listThunk<UsersState, UserDto>('users', 'users', async params => {
    const { items, total } = await Clients.Users.getUsers(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadUsers = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const loadUser = createApiThunk('users/load',
    (arg: { appId: string; userId: string }) => {
        return Clients.Users.getUser(arg.appId, arg.userId);
    });

export const upsertUser = createApiThunk('users/upsert',
    async (arg: { appId: string; params: UpsertUserDto }) => {
        const response = await Clients.Users.postUsers(arg.appId, { requests: [arg.params] });

        return response[0];
    });

export const deleteUser = createApiThunk('users/delete',
    (arg: { appId: string; userId: string }) => {
        return Clients.Users.deleteUser(arg.appId, arg.userId);
    });

export const usersMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertUser.fulfilled.match(action) || deleteUser.fulfilled.match(action)) {
        const load: any = loadUsers(action.meta.arg.appId);

        store.dispatch(load);
    }

    return result;
};

const initialState: UsersState = {
    users: list.createInitial(),
};

export const usersReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadUser.pending, (state) => {
        state.loadingUser = true;
        state.loadingUsersError = undefined;
    })
    .addCase(loadUser.rejected, (state, action) => {
        state.loadingUser = false;
        state.loadingUsersError = action.payload as ErrorDto;
        state.user = undefined;
    })
    .addCase(loadUser.fulfilled, (state, action) => {
        state.loadingUser = false;
        state.loadingUsersError = undefined;
        state.user = action.payload as any;
    })
    .addCase(upsertUser.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertUser.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertUser.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;

        if (!state.user || state.user.id === action.payload.id) {
            state.user = action.payload;
        }
    }));
