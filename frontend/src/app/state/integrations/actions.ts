/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { createExtendedReducer, createMutation, formatError } from '@app/framework';
import { Clients, CreateIntegrationDto, UpdateIntegrationDto } from '@app/service';
import { selectApp } from './../shared';
import { IntegrationsState } from './state';

export const loadIntegrations = createMutation<IntegrationsState>('loading').with({
    name: 'integrations/loading',
    mutateFn: async (arg: { appId: string }) => {
        return await Clients.Apps.getIntegrations(arg.appId);
    },
    updateFn(state, action) {
        state.configured = action.payload.configured;
        state.supported = action.payload.supported;
    },
});

export const createIntegration = createMutation<IntegrationsState>('upserting').with({
    name: 'integrations/create',
    mutateFn: async (arg: { appId: string; params: CreateIntegrationDto }) => {
        return await Clients.Apps.postIntegration(arg.appId, arg.params);
    },
    updateFn(state, action) {
        const id = action.payload.id;

        state.configured[id] = action.payload.integration;
    },
});

export const updateIntegration = createMutation<IntegrationsState>('upserting').with({
    name: 'integrations/update',
    mutateFn: async (arg: { appId: string; id: string; params: UpdateIntegrationDto }) => {
        return await Clients.Apps.putIntegration(arg.appId, arg.id, arg.params);
    },
    updateFn(state, action) {
        const id = action.meta.arg.id;

        state.configured[id] = { id, ...action.meta.arg.params } as any;
    },
});

export const deleteIntegration = createMutation<IntegrationsState>('upserting').with({
    name: 'integrations/delete',
    mutateFn: async (arg: { appId: string; id: string }) => {
        await Clients.Apps.deleteIntegration(arg.appId, arg.id);
    },
    updateFn(state, action) {
        const id = action.meta.arg.id;

        delete state.configured[id];
    },
});

export const integrationsMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createIntegration.fulfilled.match(action) || updateIntegration.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadIntegrations({ appId }) as any);
    } else if (updateIntegration.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: IntegrationsState = {
    configured: {},
    supported: {},
};

const operations = [
    loadIntegrations,
    createIntegration,
    updateIntegration,
    deleteIntegration,
];

export const integrationsReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }),
operations);
