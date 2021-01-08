/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Clients } from '@app/service';
import { createReducer } from '@reduxjs/toolkit';
import { createApiThunk } from '../shared';
import { CoreState } from './state';

export const loadTimezonesAsync = createApiThunk('core/timezones', async () => {
    const response = await Clients.Configs.getTimezones();

    return response.map(x => ({ label: x, value: x }));
});

export const loadLanguagesAsync = createApiThunk('core/languages', async () => {
    const response = await Clients.Configs.getLanguages();

    return response.map(x => ({ label: x, value: x }));
});

const initialState: CoreState = {
    timezones: [], languages: [],
};

export const coreReducer = createReducer<CoreState>(initialState, builder => builder
    .addCase(loadTimezonesAsync.fulfilled, (state, action) => {
        state.timezones = action.payload;
    })
    .addCase(loadLanguagesAsync.fulfilled, (state, action) => {
        state.languages = action.payload;
    }));
