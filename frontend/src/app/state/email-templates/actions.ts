/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { Clients, EmailTemplateDto } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from './../shared';
import { EmailTemplatesState } from './state';

export const loadEmailTemplatesAsync = createApiThunk('emailTemplates/load',
    (arg: { appId: string }) => {
        return Clients.Apps.getEmailTemplates(arg.appId);
    });

export const createEmailTemplateAsync = createApiThunk('emailTemplates/create',
    (arg: { appId: string, language: string }) => {
        return Clients.Apps.postEmailTemplate(arg.appId, { language: arg.language });
    });

export const updateEmailTemplateAsync = createApiThunk('emailTemplates/upsert',
    (arg: { appId: string, language: string, template: EmailTemplateDto }) => {
        return Clients.Apps.putEmailTemplate(arg.appId, arg.language, arg.template);
    });

export const deleteEmailTemplateAsync = createApiThunk('emailTemplates/delete',
    (arg: { appId: string, language: string }) => {
        return Clients.Apps.deleteEmailTemplate(arg.appId, arg.language);
    });

const initialState: EmailTemplatesState = { emailTemplates: {} };

export const emailTemplatesReducer = createReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(loadEmailTemplatesAsync.pending, (state) => {
        state.loading = true,
        state.loadingError = undefined;
    })
    .addCase(loadEmailTemplatesAsync.rejected, (state, action) => {
        state.loading = false;
        state.loadingError = action.payload as ErrorDto;
    })
    .addCase(loadEmailTemplatesAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.loadingError = undefined;
        state.emailTemplates = action.payload;
    })
    .addCase(createEmailTemplateAsync.pending, (state) => {
        state.creating = true,
        state.creatingError = undefined;
    })
    .addCase(createEmailTemplateAsync.rejected, (state, action) => {
        state.creating = false;
        state.creatingError = action.payload as ErrorDto;
    })
    .addCase(createEmailTemplateAsync.fulfilled, (state, action) => {
        const { language } = action.meta.arg;

        state.creating = false;
        state.creatingError = undefined;
        state.emailTemplates[language] = action.payload;
    })
    .addCase(deleteEmailTemplateAsync.pending, (state) => {
        state.deleting = true,
        state.deletingError = undefined;
    })
    .addCase(deleteEmailTemplateAsync.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = action.payload as ErrorDto;
    })
    .addCase(deleteEmailTemplateAsync.fulfilled, (state, action) => {
        const { language } = action.meta.arg;

        state.deleting = false;
        state.deletingError = undefined;
        delete state.emailTemplates[language];
    }));
