/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, listThunk, Query } from '@app/framework';
import { ChannelTemplateDto, Clients, UpdateChannelTemplateDtoOfSmsTemplateDto } from '@app/service';
import { createReducer, Middleware } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from '../shared';
import { SmsTemplatesState } from './state';

const list = listThunk<SmsTemplatesState, ChannelTemplateDto>('smsTemplates', 'templates', async params => {
    const { items, total } = await Clients.SmsTemplates.getTemplates(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadSmsTemplates = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const loadSmsTemplate = createApiThunk('smsTemplates/load',
    (arg: { appId: string; id: string }) => {
        return Clients.SmsTemplates.getTemplate(arg.appId, arg.id);
    });

export const createSmsTemplate = createApiThunk('smsTemplates/create',
    (arg: { appId: string }) => {
        return Clients.SmsTemplates.postTemplate(arg.appId, { });
    });

export const createSmsTemplateLanguage = createApiThunk('smsTemplates/createLanguage',
    (arg: { appId: string; id: string; language: string }) => {
        return Clients.SmsTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    });

export const updateSmsTemplate = createApiThunk('smsTemplates/update',
    (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfSmsTemplateDto }) => {
        return Clients.SmsTemplates.putTemplate(arg.appId, arg.id, arg.update);
    });

export const deleteSmsTemplate = createApiThunk('smsTemplates/delete',
    (arg: { appId: string; id: string }) => {
        return Clients.SmsTemplates.deleteTemplate(arg.appId, arg.id);
    });

export const smsTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createSmsTemplate.fulfilled.match(action) || deleteSmsTemplate.fulfilled.match(action)) {
        const load: any = loadSmsTemplates(action.meta.arg.appId);

        store.dispatch(load);
    }

    return result;
};

const initialState: SmsTemplatesState = {
    templates: list.createInitial(),
};

export const smsTemplatesReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadSmsTemplate.pending, (state) => {
        state.loadingTemplate = true;
        state.loadingTemplateError = undefined;
    })
    .addCase(loadSmsTemplate.rejected, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = action.payload as ErrorDto;
    })
    .addCase(loadSmsTemplate.fulfilled, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = undefined;
        state.template = action.payload;
    })
    .addCase(createSmsTemplate.pending, (state) => {
        state.creating = true;
        state.creatingError = undefined;
    })
    .addCase(createSmsTemplate.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorDto;
    })
    .addCase(createSmsTemplate.fulfilled, (state) => {
        state.creating = false;
        state.creatingError = undefined;
    })
    .addCase(updateSmsTemplate.pending, (state) => {
        state.updating = true;
        state.updatingError = undefined;
    })
    .addCase(updateSmsTemplate.rejected, (state, action) => {
        state.updating = false;
        state.updatingError = action.payload as ErrorDto;
    })
    .addCase(updateSmsTemplate.fulfilled, (state, action) => {
        state.updating = false;
        state.updatingError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            state.template = { ...state.template, ...action.meta.arg.update };
        }
    })
    .addCase(deleteSmsTemplate.pending, (state) => {
        state.deleting = true;
        state.deletingError = undefined;
    })
    .addCase(deleteSmsTemplate.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = action.payload as ErrorDto;
    })
    .addCase(deleteSmsTemplate.fulfilled, (state) => {
        state.deleting = false;
        state.deletingError = undefined;
    }));
