/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, UpdateChannelTemplateDtoOfMessagingTemplateDto } from '@app/service';
import { selectApp } from './../shared';
import { MessagingTemplatesState, MessagingTemplatesStateInStore } from './state';

export const loadMessagingTemplates = createList<MessagingTemplatesState, MessagingTemplatesStateInStore>('templates', 'messagingTemplates').with({
    name: 'messagingTemplates/load',
    queryFn: async (p: { appId: string }, q) => {
        const { items, total } = await Clients.MessagingTemplates.getTemplates(p.appId, q.search, q.take, q.skip);

        return { items, total };
    },
});

export const loadMessagingTemplate = createMutation<MessagingTemplatesState>('loadingTemplate').with({
    name: 'messagingTemplates/loadOne',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.MessagingTemplates.getTemplate(arg.appId, arg.id);
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const createMessagingTemplate = createMutation<MessagingTemplatesState>('loadingTemplate').with({
    name: 'messagingTemplates/create',
    mutateFn: (arg: { appId: string }) => {
        return Clients.MessagingTemplates.postTemplate(arg.appId, { });
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const createMessagingTemplateLanguage = createMutation<MessagingTemplatesState>('loadingTemplate').with({
    name: 'messagingTemplates/createLanguage',
    mutateFn: (arg: { appId: string; id: string; language: string }) => {
        return Clients.MessagingTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const updateMessagingTemplate = createMutation<MessagingTemplatesState>('loadingTemplate').with({
    name: 'messagingTemplates/update',
    mutateFn: (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfMessagingTemplateDto }) => {
        return Clients.MessagingTemplates.putTemplate(arg.appId, arg.id, arg.update);
    },
    updateFn(state, action) {
        if (state.template && state.template.id === action.meta.arg.id) {
            state.template = { ...state.template, ...action.meta.arg.update };
        }
    },
});

export const deleteMessagingTemplate = createMutation<MessagingTemplatesState>('loadingTemplate').with({
    name: 'messagingTemplates/delete',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.MessagingTemplates.deleteTemplate(arg.appId, arg.id);
    },
});

export const messagingTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createMessagingTemplate.fulfilled.match(action) || deleteMessagingTemplate.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadMessagingTemplates({ appId }) as any);
    } else if (deleteMessagingTemplate.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: MessagingTemplatesState = {
    templates: loadMessagingTemplates.createInitial(),
};

const operations = [
    createMessagingTemplate,
    createMessagingTemplateLanguage,
    deleteMessagingTemplate,
    loadMessagingTemplate,
    loadMessagingTemplates,
    updateMessagingTemplate,
];

export const messagingTemplatesReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }), operations);
