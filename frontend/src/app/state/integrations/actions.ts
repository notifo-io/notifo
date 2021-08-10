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

export const loadIntegrations = createApiThunk('integrations/load',
    async (arg: { appId: string }) => {
        return await Clients.Apps.getIntegrations(arg.appId);
    });

export const createIntegration = createApiThunk('integrations/create',
    async (arg: { appId: string; params: CreateIntegrationDto }) => {
        return await Clients.Apps.postIntegration(arg.appId, arg.params);
    });

export const updateIntegration = createApiThunk('integrations/update',
    async (arg: { appId: string; id: string; params: UpdateIntegrationDto }) => {
        return await Clients.Apps.putIntegration(arg.appId, arg.id, arg.params);
    });

export const deleteIntegration = createApiThunk('integrations/delete',
    async (arg: { appId: string; id: string }) => {
        await Clients.Apps.deleteIntegration(arg.appId, arg.id);
    });

const initialState: IntegrationsState = {};

export const integrationsReducer = createReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadIntegrations.pending, (state) => {
        state.loading = true;
        state.loadingError = undefined;
    })
    .addCase(loadIntegrations.rejected, (state, action) => {
        state.loading = false;
        state.loadingError = action.payload as ErrorDto;
    })
    .addCase(loadIntegrations.fulfilled, (state, action) => {
        state.configured = action.payload.configured;
        state.loading = false;
        state.loadingError = undefined;
        state.supported = action.payload.supported;
    })
    .addCase(createIntegration.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(createIntegration.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(createIntegration.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;

        if (state.configured) {
            state.configured[action.payload.id] = action.payload.integration;
        }
    })
    .addCase(updateIntegration.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(updateIntegration.rejected, (state, action) => {
        state.upserting = false;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(updateIntegration.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;

        if (state.configured) {
            const { id, params } = action.meta.arg;

            state.configured[id] = { id, ...params } as any;
        }
    })
    .addCase(deleteIntegration.fulfilled, (state, action) => {
        if (state.configured) {
            const { id } = action.meta.arg;

            delete state.configured[id];
        }
    }));
