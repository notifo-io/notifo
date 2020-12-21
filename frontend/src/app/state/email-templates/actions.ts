/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError } from '@app/framework';
import { Clients, EmailTemplateDto } from '@app/service';
import { Dispatch, Reducer } from 'redux';
import { APP_SELECTED } from './../shared';
import { EmailTemplatesState } from './state';

export const EMAIL_TEMPLATES_LOAD_STARTED = 'APPS_LOAD_TEMPLATES_STARTED';
export const EMAIL_TEMPLATES_LOAD_FAILED = 'APPS_LOAD_TEMPLATES_FAILED';
export const EMAIL_TEMPLATES_LOAD_SUCCEEEDED = 'APPS_LOAD_TEMPLATES_SUCCEEEDED';
export const EMAIL_TEMPLATE_CREATE_STARTED = 'EMAIL_TEMPLATE_CREATE_STARTED';
export const EMAIL_TEMPLATE_CREATE_FAILED = 'EMAIL_TEMPLATE_CREATE_FAILED';
export const EMAIL_TEMPLATE_CREATE_SUCCEEEDED = 'EMAIL_TEMPLATE_CREATE_SUCCEEEDED';
export const EMAIL_TEMPLATE_UPDATE_STARTED = 'EMAIL_TEMPLATE_UPDATE_STARTED';
export const EMAIL_TEMPLATE_UPDATE_FAILED = 'EMAIL_TEMPLATE_UPDATE_FAILED';
export const EMAIL_TEMPLATE_UPDATE_SUCCEEEDED = 'EMAIL_TEMPLATE_UPDATE_SUCCEEEDED';
export const EMAIL_TEMPLATE_DELETE_STARTED = 'EMAIL_TEMPLATE_DELETE_STARTED';
export const EMAIL_TEMPLATE_DELETE_FAILED = 'EMAIL_TEMPLATE_DELETE_FAILED';
export const EMAIL_TEMPLATE_DELETE_SUCCEEEDED = 'EMAIL_TEMPLATE_DELETE_SUCCEEEDED';

export const loadEmailTemplatesAsync = ({ appId }: { appId: string }) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: EMAIL_TEMPLATES_LOAD_STARTED });

        try {
            const emailTemplates = await Clients.Apps.getEmailTemplates(appId);

            dispatch({ type: EMAIL_TEMPLATES_LOAD_SUCCEEEDED, emailTemplates });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: EMAIL_TEMPLATES_LOAD_FAILED, error });
        }
    };
};

export const createEmailTemplateAsync = ({ appId, language }: { appId: string, language: string }) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: EMAIL_TEMPLATE_CREATE_STARTED });

        try {
            const emailTemplate = await Clients.Apps.postEmailTemplate(appId, { language });

            dispatch({ type: EMAIL_TEMPLATE_CREATE_SUCCEEEDED, emailTemplate, language });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: EMAIL_TEMPLATE_CREATE_FAILED, error });
        }
    };
};

export const updateEmailTemplateAsync = ({ appId, language, template }: { appId: string, language: string, template: EmailTemplateDto }) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: EMAIL_TEMPLATE_UPDATE_STARTED });

        try {
            await Clients.Apps.putEmailTemplate(appId, language, template);

            dispatch({ type: EMAIL_TEMPLATE_UPDATE_SUCCEEEDED });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: EMAIL_TEMPLATE_UPDATE_FAILED, error });
        }
    };
};

export const deleteEmailTemplateAsync = ({ appId, language }: { appId: string, language: string }) => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: EMAIL_TEMPLATE_DELETE_STARTED });

        try {
            await Clients.Apps.deleteEmailTemplate(appId, language);

            dispatch({ type: EMAIL_TEMPLATE_DELETE_SUCCEEEDED, language });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: EMAIL_TEMPLATE_DELETE_FAILED, error });
        }
    };
};

export function emailTemplatesReducer(): Reducer<EmailTemplatesState> {
    const initialState: EmailTemplatesState = { emailTemplates: {} };

    const reducer: Reducer<EmailTemplatesState> = (state = initialState, action) => {
        switch (action.type) {
            case APP_SELECTED:
                return initialState;
            case EMAIL_TEMPLATES_LOAD_STARTED:
                return {
                    ...state,
                    loading: true,
                    loadingError: null,
                };
            case EMAIL_TEMPLATES_LOAD_FAILED:
                return {
                    ...state,
                    loading: false,
                    loadingError: action.error,
                };
            case EMAIL_TEMPLATES_LOAD_SUCCEEEDED:
                return {
                    ...state,
                    loading: false,
                    loadingError: null,
                    emailTemplates: action.emailTemplates,
                };
            case EMAIL_TEMPLATE_CREATE_STARTED:
                return {
                    ...state,
                    creating: true,
                    creatingError: null,
                };
            case EMAIL_TEMPLATE_CREATE_FAILED:
                return {
                    ...state,
                    creating: false,
                    creatingError: action.error,
                };
            case EMAIL_TEMPLATE_CREATE_SUCCEEEDED:
                return {
                    ...state,
                    creating: false,
                    creatingError: null,
                    emailTemplates: withValue(state.emailTemplates, action.language, action.emailTemplate),
                };
            case EMAIL_TEMPLATE_UPDATE_STARTED:
                return {
                    ...state,
                    updating: true,
                    updatingError: null,
                };
            case EMAIL_TEMPLATE_UPDATE_FAILED:
                return {
                    ...state,
                    updating: false,
                    updatingError: action.error,
                };
            case EMAIL_TEMPLATE_UPDATE_SUCCEEEDED:
                return {
                    ...state,
                    updating: false,
                    updatingError: null,
                };
            case EMAIL_TEMPLATE_DELETE_STARTED:
                return {
                    ...state,
                    deleting: true,
                    deletingError: null,
                };
            case EMAIL_TEMPLATE_DELETE_FAILED:
                return {
                    ...state,
                    deleting: false,
                    deletingError: action.error,
                };
            case EMAIL_TEMPLATE_DELETE_SUCCEEEDED:
                return {
                    ...state,
                    deleting: false,
                    deletingError: null,
                    emailTemplates: withoutKey(state.emailTemplates, action.language),
                };
            default:
                return state;
        }
    };

    return reducer;
}

function withoutKey<T>(source: { [key: string]: T }, key: string) {
    const clone = { ...source };

    delete clone[key];

    return clone;
}

function withValue<T>(source: { [key: string]: T }, key: string, value: T) {
    const clone = { ...source };

    clone[key] = value;

    return clone;
}
