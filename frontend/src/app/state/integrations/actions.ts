/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { Clients, CreateIntegrationDto, UpdateIntegrationDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from '../shared';
import { IntegrationsState } from './state';

export const loadIntegrationAsync = createApiThunk('integrations/load',
    async (arg: { appId: string }) => {
        return await Clients.Apps.getIntegrations(arg.appId);
    });

export const createIntegrationAsync = createApiThunk('integrations/create',
    async (arg: { appId: string, params: CreateIntegrationDto }) => {
        return await Clients.Apps.postIntegration(arg.appId, arg.params);
    });

export const updateIntegrationAsync = createApiThunk('integrations/update',
    async (arg: { appId: string, id: string, params: UpdateIntegrationDto }) => {
        return await Clients.Apps.putIntegration(arg.appId, arg.id, arg.params);
    });

export const deleteIntegrationAsync = createApiThunk('integrations/delete',
    async (arg: { appId: string, id: string }) => {
        await Clients.Apps.deleteIntegration(arg.appId, arg.id);
    });

const initialState: IntegrationsState = { configured: {}, supported: {} };

export const integrationsReducer = createReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadIntegrationAsync.pending, (state) => {
        state.loading = true;
        state.loadingError = undefined;
    })
    .addCase(loadIntegrationAsync.rejected, (state, action) => {
        state.loading = false;
        state.loadingError = action.payload as ErrorDto;
    })
    .addCase(loadIntegrationAsync.fulfilled, (state, action) => {
        state.configured = action.payload.configured;
        state.loading = false;
        state.loadingError = undefined;
        state.supported = action.payload.supported;
    })
    .addCase(createIntegrationAsync.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(createIntegrationAsync.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(createIntegrationAsync.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;
        state.configured[action.payload.id] = action.payload.integration;
    })
    .addCase(updateIntegrationAsync.pending, (state, action) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(updateIntegrationAsync.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(updateIntegrationAsync.fulfilled, (state, action) => {
        const { id, params } = action.meta.arg;

        state.upserting = false;
        state.upsertingError = undefined;
        state.configured[id] = { ...state.configured[id], ...params };
    })
    .addCase(deleteIntegrationAsync.fulfilled, (state, action) => {
        const { id } = action.meta.arg;

        delete state.configured[id];
    }));
