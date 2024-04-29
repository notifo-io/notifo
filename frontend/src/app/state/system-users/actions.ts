/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { toast } from 'react-toastify';
import { Middleware } from 'redux';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, UpdateSystemUserDto } from '@app/service';
import { createApiThunk } from './../shared';
import { SystemUsersState, SystemUsersStateInStore } from './state';

export const loadSystemUsers = createList<SystemUsersState, SystemUsersStateInStore>('systemUsers', 'systemUsers').with({
    name: 'systemUsers/load',
    queryFn: async (_, p) => {
        const { items, total } = await Clients.SystemUsers.getUsers(p.search, p.take, p.skip);

        return { items, total };
    },
});

export const upsertSystemUser = createMutation<SystemUsersState>('upserting').with({
    name: 'systemUsers/upsert',
    mutateFn: async (arg: { params: UpdateSystemUserDto; userId?: string }) => {
        if (arg.userId) {
            return await Clients.SystemUsers.putUser(arg.userId, arg.params);
        } else {
            return await Clients.SystemUsers.postUser(arg.params as any);
        }
    },
});

export const lockSystemUser = createApiThunk('systemUsers/lock',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.lockUser(arg.userId);
    });

export const unlockSystemUser = createApiThunk('systemUsers/unlock',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.unlockUser(arg.userId);
    });

export const deleteSystemUser = createApiThunk('systemUsers/delete',
    (arg: { userId: string }) => {
        return Clients.SystemUsers.deleteUser(arg.userId);
    });

export const systemUsersMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertSystemUser.fulfilled.match(action) || deleteSystemUser.fulfilled.match(action)) {
        store.dispatch(loadSystemUsers({}) as any);
    } else if (deleteSystemUser.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: SystemUsersState = {
    systemUsers: loadSystemUsers.createInitial(),
};

const operations = [
    loadSystemUsers,
    upsertSystemUser,
];

export const systemUsersReducer = createExtendedReducer(initialState, builder => builder
    .addCase(lockSystemUser.fulfilled, (state, action) => {
        state.systemUsers.items?.replaceBy('id', action.payload);
    })
    .addCase(unlockSystemUser.fulfilled, (state, action) => {
        state.systemUsers.items?.replaceBy('id', action.payload);
    }),
operations);
