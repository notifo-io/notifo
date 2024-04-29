/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ActionReducerMapBuilder, createAsyncThunk, createReducer } from '@reduxjs/toolkit';
import type { NotFunction, ReducerWithInitialState } from '@reduxjs/toolkit/dist/createReducer';
import { buildError, Types } from '../utils';


export function createApiThunk<Returned, Args = void>(typePrefix: string, payloadCreator: (arg: Args) => Promise<Returned>) {
    return createAsyncThunk<Returned, Args>(typePrefix, async (arg, thunkApi) => {
        try {
            const result = await payloadCreator(arg);

            return result;
        } catch (err: any) {
            const error = buildError(err.status, err.message, err.details);

            return thunkApi.rejectWithValue(error);
        }
    });
}

export function createExtendedReducer<S extends NotFunction<any>>(
    initialState: S | (() => S), 
    builderCallback: (builder: ActionReducerMapBuilder<S>) => void,
    extensions: Function[]): ReducerWithInitialState<S> {
    return createReducer(initialState, builder => {
        builderCallback(builder);

        for (const extension of extensions) {
            const initialize = (extension as any)['initialize'];

            if (Types.isFunction(initialize)) {
                initialize(builder);
            }
        }

        return builder;
    });
}
