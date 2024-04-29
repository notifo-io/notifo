/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { toast } from 'react-toastify';
import { Middleware } from 'redux';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, UpsertUserDto } from '@app/service';
import { selectApp } from './../shared';
import { UsersState, UsersStateInStore } from './state';

export const loadUsers = createList<UsersState, UsersStateInStore>('users', 'users').with({
    name: 'users/load', 
    queryFn: async (p: { appId: string }, q) => {
        const { items, total } = await Clients.Users.getUsers(p.appId, q.search, q.take, q.skip, true);

        return { items, total };
    },
});

export const loadUser = createMutation<UsersState>('loadingUser').with({
    name: 'users/loadOne',
    mutateFn: (arg: { appId: string; userId: string }) => {
        return Clients.Users.getUser(arg.appId, arg.userId, true);
    },
    updateFn(state, action) {
        state.user = action.payload;
    },
});

export const upsertUser = createMutation<UsersState>('upserting').with({
    name: 'users/upsert',
    mutateFn: async (arg: { appId: string; params: UpsertUserDto }) => {
        const response = await Clients.Users.postUsers(arg.appId, { requests: [arg.params] });

        return response[0];
    },
    updateFn(state, action) {
        if (!state.user || state.user.id === action.payload.id) {
            state.user = action.payload;
        }
    },
});

export const deleteUserMobilePushToken = createMutation<UsersState>('upserting').with({
    name: 'users/mobilepush/delete',
    mutateFn: async (arg: { appId: string; userId: string; token: string }) => {
        return await Clients.Users.deleteMobilePushToken(arg.appId, arg.userId, arg.token);
    },
});

export const deleteUserWebPushSubscription = createMutation<UsersState>('upserting').with({
    name: 'users/webpush/delete',
    mutateFn: async (arg: { appId: string; userId: string; endpoint: string }) => {
        return await Clients.Users.deleteWebPushSubscription(arg.appId, arg.userId, arg.endpoint);
    },
});

export const deleteUser = createMutation<UsersState>('upserting').with({
    name: 'users/delete',
    mutateFn: (arg: { appId: string; userId: string }) => {
        return Clients.Users.deleteUser(arg.appId, arg.userId);
    },
});

export const usersMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (upsertUser.fulfilled.match(action) || deleteUser.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadUsers({ appId }) as any);
    } else if (
        deleteUserMobilePushToken.rejected.match(action) ||
        deleteUserWebPushSubscription.rejected.match(action) ||
        deleteUser.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    } else if (
        deleteUserMobilePushToken.fulfilled.match(action) ||
        deleteUserWebPushSubscription.fulfilled.match(action)) {
        const { appId, userId } = action.meta.arg;

        store.dispatch(loadUser({ appId, userId }) as any);
    }

    return result;
};

const initialState: UsersState = {
    users: loadUsers.createInitial(),
};

const operations = [
    deleteUser,
    deleteUserMobilePushToken,
    deleteUserWebPushSubscription,
    loadUser,
    loadUsers,
    upsertUser,
];

export const usersReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }),
operations);
