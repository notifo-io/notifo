/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createAction, createReducer } from '@reduxjs/toolkit';
import { ErrorDto, listThunk } from '@app/framework';
import { AddContributorDto, AppDto, Clients, UpsertAppDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { AppsState, CreateAppParams } from './state';

const list = listThunk<AppsState, AppDto>('apps', 'apps', async () => {
    const items = await Clients.Apps.getApps();

    return { items, total: items.length };
});

export const loadApps = () => {
    return list.action({});
};

export const loadDetails = createApiThunk('apps/load',
    (arg: { appId: string }) => {
        return Clients.Apps.getApp(arg.appId);
    });

export const createAppReset = createAction('apps/create/reset');

export const createApp = createApiThunk('apps/create',
    (arg: { params: CreateAppParams }) => {
        return Clients.Apps.postApp(arg.params);
    });

export const upsertApp = createApiThunk('apps/upsert',
    (arg: { appId: string; params: UpsertAppDto }) => {
        return Clients.Apps.putApp(arg.appId, arg.params);
    });

export const addContributor = createApiThunk('apps/contributors/add',
    (arg: { appId: string; params: AddContributorDto }) => {
        return Clients.Apps.postContributor(arg.appId, arg.params);
    });

export const removeContributor = createApiThunk('apps/contributors/remove',
    (arg: { appId: string; id: string }) => {
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
    .addCase(createApp.pending, (state) => {
        state.creating = true;
        state.creatingError = undefined;
    })
    .addCase(createApp.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorDto;
    })
    .addCase(createApp.fulfilled, (state, action) => {
        state.creating = false;
        state.creatingError = undefined;
        state.apps.items?.setOrUnshift(x => x.id, action.payload);
        state.apps.total++;
    })
    .addCase(loadDetails.pending, (state) => {
        state.loadingDetails = true;
        state.loadingDetailsError = undefined;
    })
    .addCase(loadDetails.rejected, (state, action) => {
        state.loadingDetails = false;
        state.loadingDetailsError = action.payload as ErrorDto;
    })
    .addCase(loadDetails.fulfilled, (state, action) => {
        state.loadingDetails = false;
        state.loadingDetailsError = undefined;
        state.app = action.payload;
    })
    .addCase(upsertApp.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertApp.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertApp.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;
        state.apps.items?.set(x => x.id, action.payload);

        if (state.app && state.app.id === action.payload.id) {
            state.app = action.payload;
        }
    })
    .addCase(addContributor.pending, (state) => {
        state.contributorsUpdating = true;
        state.contributorsError = undefined;
    })
    .addCase(addContributor.rejected, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = action.payload as ErrorDto;
    })
    .addCase(addContributor.fulfilled, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = undefined;
        state.apps.items?.set(x => x.id, action.payload);

        if (state.app && state.app.id === action.payload.id) {
            state.app = action.payload;
        }
    })
    .addCase(removeContributor.pending, (state) => {
        state.contributorsUpdating = true;
        state.contributorsError = undefined;
    })
    .addCase(removeContributor.rejected, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = action.payload as ErrorDto;
    })
    .addCase(removeContributor.fulfilled, (state, action) => {
        state.contributorsUpdating = false;
        state.contributorsError = undefined;
        state.apps.items?.set(x => x.id, action.payload);

        if (state.app && state.app.id === action.payload.id) {
            state.app = action.payload;
        }
    }));
