/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createAction } from '@reduxjs/toolkit';
import { createExtendedReducer, createList, createMutation } from '@app/framework';
import { Clients, TemplateDto } from '@app/service';
import { selectApp } from './../shared';
import { TemplatesState, TemplatesStateInStore } from './state';

export const loadTemplates = createList<TemplatesState, TemplatesStateInStore>('templates', 'templates').with({
    name: 'templates/load',
    queryFn: async (p: { appId: string }) => {
        const { items, total } = await Clients.Templates.getTemplates(p.appId, undefined, 1000, 0);

        return { items, total };
    },
});

export const selectTemplate = createAction<{ code: string | undefined }>('templates/select');

export const upsertTemplate = createMutation<TemplatesState>('upserting').with({
    name: 'templates/upsert',
    mutateFn: async (arg: { appId: string; params: TemplateDto }) => {
        const response = await Clients.Templates.postTemplates(arg.appId, { requests: [arg.params] });

        return response[0];
    },
    updateFn(state, action) {
        state.templates.items?.setOrPush(x => x.code, action.payload);
    },
});

export const deleteTemplate = createMutation<TemplatesState>('upserting').with({
    name: 'templates/delete',
    mutateFn: (arg: { appId: string; code: string }) => {
        return Clients.Templates.deleteTemplate(arg.appId, arg.code);
    },
    updateFn(state, action) {
        state.templates.items?.removeByValue(x => x.code, action.meta.arg.code);
    },
});

const initialState: TemplatesState = {
    templates: loadTemplates.createInitial(),
};

const operations = [
    deleteTemplate,
    loadTemplates,
    upsertTemplate,
];

export const templatesReducer = createExtendedReducer(initialState, builder => builder
    .addCase(selectApp, () => {
        return initialState;
    })
    .addCase(selectTemplate, (state, action) => {
        state.currentTemplateCode = action.payload.code;
    }),
operations);
