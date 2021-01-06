/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { buildError } from '@app/framework';
import { AsyncThunkPayloadCreator, createAction, createAsyncThunk } from '@reduxjs/toolkit';

export const selectApp = createAction<{ appId: string }>('apps/select');

export function createApiThunk<Returned, ThunkArg = void>(typePrefix: string, payloadCreator: AsyncThunkPayloadCreator<Returned, ThunkArg, any>) {
    return createAsyncThunk<Returned, ThunkArg>(typePrefix, async (arg, thunkApi) => {
        try {
            const result = await payloadCreator(arg, thunkApi);

            return result;
        } catch (err) {
            const error = buildError(err.status, err.message);

            return thunkApi.rejectWithValue(error);
        }
    });
}
