/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError } from '@app/framework';
import { Clients } from '@app/service';
import { Dispatch, Reducer } from 'redux';
import { CoreState } from './state';

export const TIMEZONES_LOAD_STARTED = 'TIMEZONES_LOAD_STARTED';
export const TIMEZONES_LOAD_FAILED = 'TIMEZONES_LOAD_FAILED';
export const TIMEZONES_LOAD_SUCCEEEDED = 'TIMEZONES_LOAD_SUCCEEEDED';
export const LANGUAGES_LOAD_STARTED = 'LANGUAGES_LOAD_STARTED';
export const LANGUAGES_LOAD_FAILED = 'LANGUAGES_LOAD_FAILED';
export const LANGUAGES_LOAD_SUCCEEEDED = 'LANGUAGES_LOAD_SUCCEEEDED';

export const loadTimezonesAsync = () => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: TIMEZONES_LOAD_STARTED });

        try {
            const response = await Clients.Configs.getTimezones();

            const timezones = response.map(x => ({ label: x, value: x }));

            dispatch({ type: TIMEZONES_LOAD_SUCCEEEDED, timezones });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: TIMEZONES_LOAD_FAILED, error });
        }
    };
};

export const loadLanguagesAsync = () => {
    return async (dispatch: Dispatch) => {
        dispatch({ type: LANGUAGES_LOAD_STARTED });

        try {
            const response = await Clients.Configs.getLanguages();

            const languages = response.map(x => ({ label: x, value: x }));

            dispatch({ type: LANGUAGES_LOAD_SUCCEEEDED, languages });
        } catch (err) {
            const error = buildError(err.status, err.message);

            dispatch({ type: LANGUAGES_LOAD_FAILED, error });
        }
    };
};

export function coreReducer(): Reducer<CoreState> {
    const initialState: CoreState = { timezones: [], languages: [] };

    const reducer: Reducer<CoreState> = (state = initialState, action) => {
        switch (action.type) {
            case LANGUAGES_LOAD_SUCCEEEDED:
                return {
                    ...state,
                    languages: action.languages,
                };
            case TIMEZONES_LOAD_SUCCEEEDED:
                return {
                    ...state,
                    timezones: action.timezones,
                };
            default:
                return state;
        }
    };

    return reducer;
}
