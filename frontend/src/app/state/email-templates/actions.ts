/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Draft, Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { createExtendedReducer, createList, createMutation, formatError } from '@app/framework';
import { ChannelTemplateDetailsDtoOfEmailTemplateDto, Clients, EmailTemplateDto, UpdateChannelTemplateDtoOfEmailTemplateDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { EmailTemplatesState, EmailTemplatesStateInStore } from './state';

export const loadEmailTemplates = createList<EmailTemplatesState, EmailTemplatesStateInStore>('templates', 'emailTemplates').with({
    name: 'emailTemplates/load',
    queryFn: async (p: { appId: string }, q) => {
        const { items, total } = await Clients.EmailTemplates.getTemplates(p.appId, q.search, q.take, q.skip);
    
        return { items, total };
    },
});

export const loadMjmlSchema = createApiThunk('emailTemplates/schema',
    () => {
        return Clients.EmailTemplates.getSchema();
    });

export const loadEmailTemplate = createMutation<EmailTemplatesState>('loadingTemplate').with({
    name: 'emailTemplates/loadOne',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.EmailTemplates.getTemplate(arg.appId, arg.id);
    },
    updateFn(state, action) {
        state.template = action.payload;
    },
});

export const createEmailTemplate = createMutation<EmailTemplatesState>('creating').with({
    name: 'emailTemplates/create',
    mutateFn: (arg: { appId: string; kind?: string }) => {
        return Clients.EmailTemplates.postTemplate(arg.appId, { kind: arg.kind });
    },
});

export const createEmailTemplateLanguage = createMutation<EmailTemplatesState>('creatingLanguage').with({
    name: 'emailTemplates/createLanguage',
    mutateFn: (arg: { appId: string; id: string; language: string }) => {
        return Clients.EmailTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    },
    updateFn(state, action) {
        updateTemplate(state, action.meta.arg.id, action.payload);
    },
});

export const updateEmailTemplate = createMutation<EmailTemplatesState>('updating').with({
    name: 'emailTemplates/update',
    mutateFn: (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfEmailTemplateDto }) => {
        return Clients.EmailTemplates.putTemplate(arg.appId, arg.id, arg.update);
    },
    updateFn(state, action) {
        updateTemplate(state, action.meta.arg.id, action.payload);
    },
});

export const updateEmailTemplateLanguage = createMutation<EmailTemplatesState>('updatingLanguage').with({
    name: 'emailTemplates/updateLanguage',
    mutateFn: (arg: { appId: string; id: string; language: string; template: EmailTemplateDto }) => {
        return Clients.EmailTemplates.putTemplateLanguage(arg.appId, arg.id, arg.language, arg.template);
    },
    updateFn(state, action) {
        updateTemplate(state, action.meta.arg.id, action.payload);
    },
});

export const deleteEmailTemplate = createMutation<EmailTemplatesState>('deleting').with({
    name: 'emailTemplates/delete',
    mutateFn: (arg: { appId: string; id: string }) => {
        return Clients.EmailTemplates.deleteTemplate(arg.appId, arg.id);
    },
});

export const deleteEmailTemplateLanguage = createMutation<EmailTemplatesState>('deletingLanguage').with({
    name: 'emailTemplates/deleteLanguage',
    mutateFn: (arg: { appId: string; id: string; language: string }) => {
        return Clients.EmailTemplates.deleteTemplateLanguage(arg.appId, arg.id, arg.language);
    },
    updateFn(state, action) {
        updateTemplate(state, action.meta.arg.id, action.payload);
    },
});

export const emailTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createEmailTemplate.fulfilled.match(action) || deleteEmailTemplate.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadEmailTemplates({ appId }) as any);
    } else if (
        deleteEmailTemplate.rejected.match(action) ||
        deleteEmailTemplateLanguage.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: EmailTemplatesState = {
    templates: loadEmailTemplates.createInitial(),
};

const operations = [
    createEmailTemplate,
    createEmailTemplateLanguage,
    deleteEmailTemplate,
    deleteEmailTemplateLanguage,
    loadEmailTemplate,
    loadEmailTemplates,
    updateEmailTemplate,
    updateEmailTemplateLanguage,
];

export const emailTemplatesReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadMjmlSchema.fulfilled, (state, action) => {
        state.schema = action.payload;
    }),
operations);

function updateTemplate(state: Draft<EmailTemplatesState>, id: string, template: ChannelTemplateDetailsDtoOfEmailTemplateDto) {
    if (state.template && state.template.id === id) {
        state.template = template;
    }
}
