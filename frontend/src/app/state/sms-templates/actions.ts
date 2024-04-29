/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { Clients, UpdateChannelTemplateDtoOfSmsTemplateDto } from '@app/service';
import { selectApp } from './../shared';
import { SmsTemplatesState, SmsTemplatesStateInStore } from './state';

export const loadSmsTemplates = createList<SmsTemplatesState, SmsTemplatesStateInStore>('templates', 'smsTemplates').with({
    name: 'smsTemplates/load',
    queryFn: async (p: { appId: string }, q) => {
        const { items, total } = await Clients.SmsTemplates.getTemplates(p.appId, q.search, q.take, q.skip);

        return { items, total };
    },
});

export const loadSmsTemplate = createMutation<SmsTemplatesState>('loadingTemplate').with({
    name: 'smsTemplates/loadOne',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.SmsTemplates.getTemplate(arg.appId, arg.id);
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const createSmsTemplate = createMutation<SmsTemplatesState>('loadingTemplate').with({
    name: 'smsTemplates/create',
    mutateFn: (arg: { appId: string }) => {
        return Clients.SmsTemplates.postTemplate(arg.appId, { });
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const createSmsTemplateLanguage = createMutation<SmsTemplatesState>('loadingTemplate').with({
    name: 'smsTemplates/createLanguage',
    mutateFn: (arg: { appId: string; id: string; language: string }) => {
        return Clients.SmsTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const updateSmsTemplate = createMutation<SmsTemplatesState>('loadingTemplate').with({
    name: 'smsTemplates/update',
    mutateFn: (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfSmsTemplateDto }) => {
        return Clients.SmsTemplates.putTemplate(arg.appId, arg.id, arg.update);
    },
    updateFn(state, action) {
        if (state.template && state.template.id === action.meta.arg.id) {
            state.template = { ...state.template, ...action.meta.arg.update };
        }
    },
});

export const deleteSmsTemplate = createMutation<SmsTemplatesState>('loadingTemplate').with({
    name: 'smsTemplates/delete',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.SmsTemplates.deleteTemplate(arg.appId, arg.id);
    },
});

export const smsTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createSmsTemplate.fulfilled.match(action) || deleteSmsTemplate.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadSmsTemplates({ appId }) as any);
    } else if (deleteSmsTemplate.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: SmsTemplatesState = {
    templates: loadSmsTemplates.createInitial(),
};

const operations = [
    createSmsTemplate,
    createSmsTemplateLanguage,
    deleteSmsTemplate,
    loadSmsTemplate,
    loadSmsTemplates,
    updateSmsTemplate,
];

export const smsTemplatesReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    }), operations);
