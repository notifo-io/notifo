/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Draft } from '@reduxjs/toolkit';
import { createExtendedReducer, createList, createMutation } from '@app/framework';
import { AddContributorDto, AppDetailsDto, AuthSchemeDto, Clients, UpsertAppDto } from '@app/service';
import { selectApp } from './../shared';
import { AppsState, AppsStateInStore, CreateAppParams } from './state';

export const loadApps = createList<AppsState, AppsStateInStore>('apps', 'apps').with({
    name: 'apps/load',
    queryFn: async () => {
        const items = await Clients.Apps.getApps();
    
        return { items, total: items.length };
    },
});

export const loadDetails = createMutation<AppsState>('loadingDetails').with({
    name: 'apps/loadOne',
    mutateFn: async (arg: { appId: string }) => {
        const [app, auth] = await Promise.all([
            Clients.Apps.getApp(arg.appId),
            Clients.Apps.getAuthScheme(arg.appId),
        ]);

        return { app, auth };
    },
    updateFn(state, action) {
        state.app = action.payload.app;
        state.auth = action.payload.auth;
    },
});

export const createApp = createMutation<AppsState>('creating').with({
    name: 'apps/create',
    mutateFn: (arg: { params: CreateAppParams }) => {
        return Clients.Apps.postApp(arg.params);
    },
    updateFn(state, action) {
        state.apps.items?.setOrUnshift(x => x.id, action.payload);
        state.apps.total++;
    },
});

export const updateApp = createMutation<AppsState>('updating').with({
    name: 'apps/upsert',
    mutateFn: (arg: { appId: string; params: UpsertAppDto }) => { 
        return Clients.Apps.putApp(arg.appId, arg.params);
    },
    updateFn(state, action) {
        updateDetails(state, action.payload);
    },
});

export const addContributor = createMutation<AppsState>('updatingContributors').with({
    name: 'apps/contributors/add',
    mutateFn: (arg: { appId: string; params: AddContributorDto }) => {
        return Clients.Apps.postContributor(arg.appId, arg.params);
    },
    updateFn(state, action) {
        updateDetails(state, action.payload);
    },
});

export const removeContributor = createMutation<AppsState>('updatingContributors').with({
    name: 'apps/contributors/remove',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.Apps.deleteContributor(arg.appId, arg.id);
    },
    updateFn(state, action) {
        updateDetails(state, action.payload);
    },
});

export const upsertAuth = createMutation<AppsState>('updatingAuth').with({
    name: 'apps/auth/upsert',
    mutateFn: (arg: { appId: string; params: AuthSchemeDto }) => {
        return Clients.Apps.upsertAuthScheme(arg.appId, arg.params);
    },
    updateFn(state, action) {
        state.auth = action.payload;
    },
});

export const removeAuth = createMutation<AppsState>('updatingAuth').with({
    name: 'apps/auth/remove',
    mutateFn: (arg: { appId: string }) =>{
        return Clients.Apps.deleteAuthScheme(arg.appId);
    },
    updateFn(state) {
        state.auth = undefined;
    },
});

const initialState: AppsState = {
    apps: loadApps.createInitial(),
};

const operations =  [
    addContributor,
    createApp,
    loadApps,
    loadDetails,
    removeAuth,
    updateApp,
    upsertAuth,
];

export const appsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, (state, action) => {
        state.appId = action.payload.appId;
    }),
operations);

function updateDetails(state: Draft<AppsState>, details: AppDetailsDto) {
    state.apps.items?.set(x => x.id, details);

    if (state.app && state.app.id === details.id) {
        state.app = details;
    }
}
