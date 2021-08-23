/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, listThunk, Query } from '@app/framework';
import { ChannelTemplateDto, Clients, UpdateChannelTemplateDtoOfMessagingTemplateDto } from '@app/service';
import { createReducer, Middleware } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from '../shared';
import { MessagingTemplatesState } from './state';

const list = listThunk<MessagingTemplatesState, ChannelTemplateDto>('messagingTemplates', 'templates', async params => {
    const { items, total } = await Clients.MessagingTemplates.getTemplates(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadMessagingTemplates = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const loadMessagingTemplate = createApiThunk('messagingTemplates/load',
    (arg: { appId: string; id: string }) => {
        return Clients.MessagingTemplates.getTemplate(arg.appId, arg.id);
    });

export const createMessagingTemplate = createApiThunk('messagingTemplates/create',
    (arg: { appId: string }) => {
        return Clients.MessagingTemplates.postTemplate(arg.appId, { });
    });

export const createMessagingTemplateLanguage = createApiThunk('messagingTemplates/createLanguage',
    (arg: { appId: string; id: string; language: string }) => {
        return Clients.MessagingTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    });

export const updateMessagingTemplate = createApiThunk('messagingTemplates/update',
    (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfMessagingTemplateDto }) => {
        return Clients.MessagingTemplates.putTemplate(arg.appId, arg.id, arg.update);
    });

export const deleteMessagingTemplate = createApiThunk('messagingTemplates/delete',
    (arg: { appId: string; id: string }) => {
        return Clients.MessagingTemplates.deleteTemplate(arg.appId, arg.id);
    });

export const messagingTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createMessagingTemplate.fulfilled.match(action) || deleteMessagingTemplate.fulfilled.match(action)) {
        const load: any = loadMessagingTemplates(action.meta.arg.appId);

        store.dispatch(load);
    }

    return result;
};

const initialState: MessagingTemplatesState = {
    templates: list.createInitial(),
};

export const messagingTemplatesReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadMessagingTemplate.pending, (state) => {
        state.loadingTemplate = true;
        state.loadingTemplateError = undefined;
    })
    .addCase(loadMessagingTemplate.rejected, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = action.payload as ErrorDto;
    })
    .addCase(loadMessagingTemplate.fulfilled, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = undefined;
        state.template = action.payload;
    })
    .addCase(createMessagingTemplate.pending, (state) => {
        state.creating = true;
        state.creatingError = undefined;
    })
    .addCase(createMessagingTemplate.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorDto;
    })
    .addCase(createMessagingTemplate.fulfilled, (state) => {
        state.creating = false;
        state.creatingError = undefined;
    })
    .addCase(updateMessagingTemplate.pending, (state) => {
        state.updating = true;
        state.updatingError = undefined;
    })
    .addCase(updateMessagingTemplate.rejected, (state, action) => {
        state.updating = false;
        state.updatingError = action.payload as ErrorDto;
    })
    .addCase(updateMessagingTemplate.fulfilled, (state, action) => {
        state.updating = false;
        state.updatingError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            state.template = { ...state.template, ...action.meta.arg.update };
        }
    })
    .addCase(deleteMessagingTemplate.pending, (state) => {
        state.deleting = true;
        state.deletingError = undefined;
    })
    .addCase(deleteMessagingTemplate.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = action.payload as ErrorDto;
    })
    .addCase(deleteMessagingTemplate.fulfilled, (state) => {
        state.deleting = false;
        state.deletingError = undefined;
    }));
