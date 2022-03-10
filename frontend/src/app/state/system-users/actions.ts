/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer } from '@reduxjs/toolkit';
import { Middleware } from 'redux';
import { ErrorDto, listThunk, Query } from '@app/framework';
import { Clients, SystemUserDto, UpdateSystemUserDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { SystemUsersState } from './state';

const list = listThunk<SystemUsersState, SystemUserDto>('systemUsers', 'systemUsers', async params => {
    const { items, total } = await Clients.SystemUsers.getUsers(params.search, params.take, params.skip);

    return { items, total };
});

export const loadSystemUsers = (query?: Partial<Query>, reset = false) => {
    return list.action({ query, reset });
};

export const upsertSystemUser = createApiThunk('system-users/upsert',
    async (arg: { params: UpdateSystemUserDto; userId?: string }) => {
        if (arg.userId) {
            return await Clients.SystemUsers.putUser(arg.userId, arg.params);
        } else {
            return await Clients.SystemUsers.postUser(arg.params);
        }
    });

export const lockSystemUser = createApiThunk('system-users/lock',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.lockUser(arg.userId);
    });

export const unlockSystemUser = createApiThunk('system-users/unlock',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.unlockUser(arg.userId);
    });

export const deleteSystemUser = createApiThunk('system-users/delete',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.deleteUser(arg.userId);
    });

export const systemUsersMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertSystemUser.fulfilled.match(action) || deleteSystemUser.fulfilled.match(action)) {
        const load: any = loadSystemUsers();

        store.dispatch(load);
    }

    return result;
};

const initialState: SystemUsersState = {
    systemUsers: list.createInitial(),
};

export const systemUsersReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(lockSystemUser.fulfilled, (state, action) => {
        state.systemUsers.items?.replaceBy('id', action.payload);
    })
    .addCase(unlockSystemUser.fulfilled, (state, action) => {
        state.systemUsers.items?.replaceBy('id', action.payload);
    })
    .addCase(upsertSystemUser.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertSystemUser.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertSystemUser.fulfilled, (state) => {
        state.upserting = false;
        state.upsertingError = undefined;
    }));
