/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, listThunk } from '@app/framework';
import { Clients, TemplateDto } from '@app/service';
import { createAction, createReducer } from '@reduxjs/toolkit';
import { createApiThunk, selectApp } from './../shared';
import { TemplatesState } from './state';

const list = listThunk<TemplatesState, TemplateDto>('templates', 'templates', async params => {
    const { items, total } = await Clients.Templates.getTemplates(params.appId, null, 1000, 0);

    return { items, total };
});

export const selectTemplate = createAction<{ code: string | undefined }>('templates/select');

export const loadTemplatesAsync = (appId: string, reset = false) => {
    return list.action({ appId, reset });
};

export const upsertTemplateAsync = createApiThunk('templates/upsert',
    async (arg: { appId: string; params: TemplateDto }) => {
        const response = await Clients.Templates.postTemplates(arg.appId, { requests: [arg.params] });

        return response[0];
    });

export const deleteTemplateAsync = createApiThunk('templates/delete',
    (arg: { appId: string; code: string }) => {
        return Clients.Templates.deleteTemplate(arg.appId, arg.code);
    });

const initialState: TemplatesState = {
    templates: list.createInitial(),
};

export const templatesReducer = createReducer(initialState, builder => list.initialize(builder)
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(selectTemplate, (state, action) => {
        state.currentTemplateCode = action.payload.code;
    })
    .addCase(upsertTemplateAsync.pending, (state) => {
        state.upserting = true;
        state.upsertingError = undefined;
    })
    .addCase(upsertTemplateAsync.rejected, (state, action) => {
        state.upserting = true;
        state.upsertingError = action.payload as ErrorDto;
    })
    .addCase(upsertTemplateAsync.fulfilled, (state, action) => {
        state.upserting = false;
        state.upsertingError = undefined;
        state.templates.items?.setOrPush(x => x.code, action.payload);
    })
    .addCase(deleteTemplateAsync.fulfilled, (state, action) => {
        state.templates.items?.removeBy(x => x.code, action.meta.arg.code);
    }));
