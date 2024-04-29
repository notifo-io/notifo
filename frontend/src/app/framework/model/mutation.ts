/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ActionReducerMapBuilder, AsyncThunk, createAction, Draft, PayloadAction } from '@reduxjs/toolkit';
import { ErrorInfo } from '../utils';
import { createApiThunk } from './shared';

export interface MutationState {
    // True, if running.
    isRunning?: boolean;

    // The error state.
    error?: ErrorInfo;
}

export interface MutationProps<State, Returned, Args> {
    // The name of the action.
    name: string;

    // The function to make the actual mutation on the server.
    mutateFn: (args: Args) => Promise<Returned>;

    // The state update function.
    updateFn?: (state: Draft<State>, action: PayloadAction<Returned, any, { arg: Args }>) => void;
}

type Mutation<Args> = AsyncThunk<any, Args, any> & {
    // Resets the state.
    reset: () => void;
};

type ConfigFunction<State> = {
    with: <Returned, Args = void>(props: MutationProps<State, Returned, Args>) => Mutation<Args>;
};

export function createMutation<State>(key: keyof State): ConfigFunction<State> {
    function configure<Returned, Args>(props: MutationProps<State, Returned, Args>) {
        const thunk = createApiThunk(props.name, props.mutateFn);
        const reset = createAction(`${props.name}/reset`);

        (thunk as any)['initialize'] = (builder: ActionReducerMapBuilder<State>) => {
            builder.addCase(thunk.fulfilled, (state, action) => {
                (state as any)[key] = { isRunning: false };
                props.updateFn?.(state, action);
            });
        
            builder.addCase(thunk.pending, (state) => {
                (state as any)[key] = { isRunning: true };
            });
        
            builder.addCase(thunk.rejected, (state, action) => {
                (state as any)[key] = { error: action.payload as ErrorInfo };
            });
        
            builder.addCase(reset, (state) => {
                (state as any)[key] = undefined;
            });
        };

        (thunk as any)['reset'] = reset;

        return thunk as any;
    }

    return { with: configure };
}