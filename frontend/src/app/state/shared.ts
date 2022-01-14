/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { AsyncThunkPayloadCreator, createAction, createAsyncThunk } from '@reduxjs/toolkit';
import { buildError } from '@app/framework';

export const selectApp = createAction<{ appId: string | undefined }>('apps/select');

export function createApiThunk<Returned, ThunkArg = void>(typePrefix: string, payloadCreator: AsyncThunkPayloadCreator<Returned, ThunkArg, any>) {
    return createAsyncThunk<Returned, ThunkArg>(typePrefix, async (arg, thunkApi) => {
        try {
            const result = await payloadCreator(arg, thunkApi);

            return result;
        } catch (err: any) {
            const error = buildError(err.status, err.message, err.details);

            return thunkApi.rejectWithValue(error);
        }
    });
}
