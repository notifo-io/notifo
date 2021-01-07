/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, listThunk } from '@app/framework';
import { AddContributorDto, AppDto, Clients, UpsertAppDto } from '@app/service';
import { createAction, createReducer } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from './../shared';
import { AppsState, CreateAppParams } from './state';

const list = listThunk<AppsState, AppDto>('apps', 'apps', async () => {
    const items = await Clients.Apps.getApps();

    return { items, total: items.length };
});

export const loadAppsAsync = () => {
    return list.action({});
};

export const loadDetailsAsync = createApiThunk('apps/load',
    (arg: { appId: string }) => {
        return Clients.Apps.getApp(arg.appId);
    });

export const createAppReset = createAction('apps/create/reset');

export const createAppAsync = createApiThunk('apps/create',
    (arg: { params: CreateAppParams }) => {
        return Clients.Apps.postApp(arg.params);
    });

export const upsertAppAsync = createApiThunk('apps/upsert',
    (arg: { appId: string, params: UpsertAppDto }) => {
        return Clients.Apps.putApp(arg.appId, arg.params);
    });

export const addContributorAsync = createApiThunk('apps/contributors/add',
    (arg: { appId: string, params: AddContributorDto }) => {
        return Clients.Apps.postContributor(arg.appId, arg.params);
    });

export const removeContributorAsync = createApiThunk('apps/contributors/remove',
    (arg: { appId: string, id: string }) => {
        return Clients.Apps.deleteContributor(arg.appId, arg.id);
    });

const initialState: AppsState = {
    apps: list.createInitial(),
};

export const appsReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, (state, action) => {
        state.appId = action.payload.appId;
    })
    .addCase(createAppReset, (state) => {
        state.creating = false;
        state.creatingError = undefined;
    })
    .addCase(createAppAsync.pending, (state) => {
        state.creating = true;
        state.creatingError = undefined;
    })
    .addCase(createAppAsync.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorDto;
    })
    .addCase(createAppAsync.fulfilled, (state, action) => {
        state.creating = false;
        state.creatingError = undefined;
        state.apps.items.setOrUnshift(x => x.id, action.payload);
        state.apps.total++;
    })
    .addCase(loadDetailsAsync.pending, (state) => {
        state.loadingDetails = true;
        state.loadingDetailsError = undefined;
    })
    .addCase(loadDetailsAsync.rejected, (state, action) => {
        state.loadingDetails = false;
        state.loadingDetailsError = action.payload as ErrorDto;
    })
    .addCase(loadDetailsAsync.fulfilled, (state, action) => {
        state.loadingDetails = false;
        state.loadingDetailsError = undefined;
        state.appDetails = action.payload;
    })
    .addCase(upsertAppAsync.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertAppAsync.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertAppAsync.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;
        state.apps.items.set(x => x.id, action.payload);
        state.appDetails = action.payload;
    })
    .addCase(addContributorAsync.pending, (state) => {
        state.contributorsUpdating = true;
        state.contributorsError = undefined;
    })
    .addCase(addContributorAsync.rejected, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = action.payload as ErrorDto;
    })
    .addCase(addContributorAsync.fulfilled, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = undefined;
        state.apps.items.set(x => x.id, action.payload);
        state.appDetails = action.payload;
    })
    .addCase(removeContributorAsync.pending, (state) => {
        state.contributorsUpdating = true;
        state.contributorsError = undefined;
    })
    .addCase(removeContributorAsync.rejected, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = action.payload as ErrorDto;
    })
    .addCase(removeContributorAsync.fulfilled, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = undefined;
        state.apps.items.set(x => x.id, action.payload);
        state.appDetails = action.payload;
    }));
