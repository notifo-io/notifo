/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createReducer, Middleware } from '@reduxjs/toolkit';
import { toast } from 'react-toastify';
import { ErrorInfo, formatError, listThunk, Query } from '@app/framework';
import { ChannelTemplateDto, Clients, EmailTemplateDto, UpdateChannelTemplateDtoOfEmailTemplateDto } from '@app/service';
import { createApiThunk, selectApp } from './../shared';
import { EmailTemplatesState } from './state';

const list = listThunk<EmailTemplatesState, ChannelTemplateDto>('emailTemplates', 'templates', async params => {
    const { items, total } = await Clients.EmailTemplates.getTemplates(params.appId, params.search, params.take, params.skip);

    return { items, total };
});

export const loadEmailTemplates = (appId: string, query?: Partial<Query>, reset = false) => {
    return list.action({ appId, query, reset });
};

export const loadEmailTemplate = createApiThunk('emailTemplates/load',
    (arg: { appId: string; id: string }) => {
        return Clients.EmailTemplates.getTemplate(arg.appId, arg.id);
    });

export const createEmailTemplate = createApiThunk('emailTemplates/create',
    (arg: { appId: string; kind?: string }) => {
        return Clients.EmailTemplates.postTemplate(arg.appId, { kind: arg.kind });
    });

export const createEmailTemplateLanguage = createApiThunk('emailTemplates/createLanguage',
    (arg: { appId: string; id: string; language: string }) => {
        return Clients.EmailTemplates.postTemplateLanguage(arg.appId, arg.id, { language: arg.language });
    });

export const updateEmailTemplate = createApiThunk('emailTemplates/update',
    (arg: { appId: string; id: string; update: UpdateChannelTemplateDtoOfEmailTemplateDto }) => {
        return Clients.EmailTemplates.putTemplate(arg.appId, arg.id, arg.update);
    });

export const updateEmailTemplateLanguage = createApiThunk('emailTemplates/updateLanguage',
    (arg: { appId: string; id: string; language: string; template: EmailTemplateDto }) => {
        return Clients.EmailTemplates.putTemplateLanguage(arg.appId, arg.id, arg.language, arg.template);
    });

export const deleteEmailTemplate = createApiThunk('emailTemplates/delete',
    (arg: { appId: string; id: string }) => {
        return Clients.EmailTemplates.deleteTemplate(arg.appId, arg.id);
    });

export const deleteEmailTemplateLanguage = createApiThunk('emailTemplates/deleteLanguage',
    (arg: { appId: string; id: string; language: string }) => {
        return Clients.EmailTemplates.deleteTemplateLanguage(arg.appId, arg.id, arg.language);
    });

export const emailTemplatesMiddleware: Middleware = store => next => action => {
    const result = next(action);

    if (createEmailTemplate.fulfilled.match(action) || deleteEmailTemplate.fulfilled.match(action)) {
        const { appId } = action.meta.arg;

        store.dispatch(loadEmailTemplates(appId) as any);
    } else if (
        deleteEmailTemplate.rejected.match(action) ||
        deleteEmailTemplateLanguage.rejected.match(action)) {
        toast.error(formatError(action.payload as any));
    }

    return result;
};

const initialState: EmailTemplatesState = {
    templates: list.createInitial(),
};

export const emailTemplatesReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadEmailTemplate.pending, (state) => {
        state.loadingTemplate = true;
        state.loadingTemplateError = undefined;
    })
    .addCase(loadEmailTemplate.rejected, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = action.payload as ErrorInfo;
    })
    .addCase(loadEmailTemplate.fulfilled, (state, action) => {
        state.loadingTemplate = false;
        state.loadingTemplateError = undefined;
        state.template = action.payload;
    })
    .addCase(createEmailTemplate.pending, (state) => {
        state.creating = true;
        state.creatingError = undefined;
    })
    .addCase(createEmailTemplate.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorInfo;
    })
    .addCase(createEmailTemplate.fulfilled, (state) => {
        state.creating = false;
        state.creatingError = undefined;
    })
    .addCase(createEmailTemplateLanguage.pending, (state) => {
        state.creatingLanguage = true;
        state.creatingLanguageError = undefined;
    })
    .addCase(createEmailTemplateLanguage.rejected, (state, action) => {
        state.creatingLanguage = false;
        state.creatingLanguageError = action.payload as ErrorInfo;
    })
    .addCase(createEmailTemplateLanguage.fulfilled, (state, action) => {
        state.creatingLanguage = false;
        state.creatingLanguageError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            state.template.languages[action.meta.arg.language] = action.payload;
        }
    })
    .addCase(updateEmailTemplate.pending, (state) => {
        state.updating = true;
        state.updatingError = undefined;
    })
    .addCase(updateEmailTemplate.rejected, (state, action) => {
        state.updating = false;
        state.updatingError = action.payload as ErrorInfo;
    })
    .addCase(updateEmailTemplate.fulfilled, (state, action) => {
        state.updating = false;
        state.updatingError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            state.template = { ...state.template, ...action.meta.arg.update };
        }
    })
    .addCase(updateEmailTemplateLanguage.pending, (state) => {
        state.updatingLanguage = true;
        state.updatingLanguageError = undefined;
    })
    .addCase(updateEmailTemplateLanguage.rejected, (state, action) => {
        state.updatingLanguage = false;
        state.updatingLanguageError = action.payload as ErrorInfo;
    })
    .addCase(updateEmailTemplateLanguage.fulfilled, (state, action) => {
        state.updatingLanguage = false;
        state.updatingLanguageError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            state.template.languages[action.meta.arg.language] = action.meta.arg.template;
        }
    })
    .addCase(deleteEmailTemplate.pending, (state) => {
        state.deleting = true;
        state.deletingError = undefined;
    })
    .addCase(deleteEmailTemplate.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = action.payload as ErrorInfo;
    })
    .addCase(deleteEmailTemplate.fulfilled, (state) => {
        state.deleting = false;
        state.deletingError = undefined;
    })
    .addCase(deleteEmailTemplateLanguage.pending, (state) => {
        state.deletingLanguage = true;
        state.deletingLanguageError = undefined;
    })
    .addCase(deleteEmailTemplateLanguage.rejected, (state, action) => {
        state.deletingLanguage = false;
        state.deletingLanguageError = action.payload as ErrorInfo;
    })
    .addCase(deleteEmailTemplateLanguage.fulfilled, (state, action) => {
        state.deletingLanguage = false;
        state.deletingLanguageError = undefined;

        if (state.template && state.template.id === action.meta.arg.id) {
            delete state.template.languages[action.meta.arg.language];
        }
    }));
