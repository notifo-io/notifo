/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { addOrModify, buildError, List, remove } from '@app/framework';
import { Clients, TemplateDto } from '@app/service';
import { Dispatch, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { TemplatesState } from './state';

export const TEMPLATES_SELECT = 'TEMPLATES_SELECT';
export const TEMPLATE_UPSERT_STARTED = 'TEMPLATE_UPSERT_STARTED';
export const TEMPLATE_UPSERT_FAILED = 'TEMPLATE_UPSERT_FAILED';
export const TEMPLATE_UPSERT_SUCCEEEDED = 'TEMPLATE_UPSERT_SUCCEEEDED';
export const TEMPLATE_DELETE_STARTED = 'TEMPLATE_DELETE_STARTED';
export const TEMPLATE_DELETE_FAILED = 'TEMPLATE_DELETE_FAILED';
export const TEMPLATE_DELETE_SUCCEEEDED = 'TEMPLATE_DELETE_SUCCEEEDED';

const list = new List<TemplateDto>('templates', 'templates', async (params) => {
    const { items, total } = await Clients.Templates.getTemplates(params.appId, null, 1000, 0);

    return { items, total };
});

export const selectTemplate = (templateCode?: string) => {
    return { type: TEMPLATES_SELECT, templateCode };
};

export const loadTemplatesAsync = (appId: string, reset = false) => {
    return list.load({}, { appId }, reset);
};

export const upsertTemplateAsync = (appId: string, params: TemplateDto) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: TEMPLATE_UPSERT_STARTED });

        try {
            const response = await Clients.Templates.postTemplates(appId, { requests: [params] });

            const template = response[0];

            dispatch({ type: TEMPLATE_UPSERT_SUCCEEEDED, template });

        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: TEMPLATE_UPSERT_FAILED, error });
        }
    };
};

export const deleteTemplateAsync = (appId: string, code: string) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: TEMPLATE_DELETE_FAILED });

        try {
            await Clients.Templates.deleteTemplate(appId, code);

            dispatch({ type: TEMPLATE_DELETE_SUCCEEEDED, appId, code });

        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: TEMPLATE_DELETE_SUCCEEEDED, error });
        }
    };
};

export function templatesReducer(): Reducer<TemplatesState> {
    const initialState: TemplatesState = {
        templates: list.createInitial(),
    };

    const reducer: Reducer<TemplatesState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            case TEMPLATES_SELECT:
                return {
                    ...state,
                    currentTemplateCode: action.templateCode,
                };
            case TEMPLATE_UPSERT_STARTED:
                return {
                    ...state,
                    upserting: true,
                    upsertingError: null,
                };
            case TEMPLATE_UPSERT_FAILED:
                return {
                    ...state,
                    upserting: false,
                    upsertingError: action.error,
                };
            case TEMPLATE_UPSERT_SUCCEEEDED: {
                return {
                    ...state,
                    upserting: false,
                    upsertingError: null,
                    templates: list.changeItems(state, items => addOrModify(items, action.template, 'code')),
                };
            }
            case TEMPLATE_DELETE_SUCCEEEDED: {
                return {
                    ...state,
                    templates: list.changeItems(state, items => remove(items, action.code, 'code')),
                };
            }
            default:
                return list.handleAction(state, action);
        }
    };

    return reducer;
}
