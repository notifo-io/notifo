/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { Clients, PublishDto } from '@app/service';
import { createAction, createReducer } from '@reduxjs/toolkit';
import { createApiThunk } from '../shared';
import { PublishState } from './state';

export const togglePublishDialog = createAction<{ open: boolean; values?: Partial<PublishDto> }>('publish/dialog');

export const publish = createApiThunk('publish/publish',
    async (arg: { appId: string; params: PublishDto }) => {
        await Clients.Events.postEvents(arg.appId, { requests: [arg.params] });
    });

const initialState: PublishState = {};

export const publishReducer = createReducer(initialState, builder => builder
    .addCase(togglePublishDialog, (state, action) => {
        state.dialogOpen = action.payload.open;
        state.dialogValues = action.payload?.values;
        state.publishing = false;
        state.publishingError = undefined;
    })
    .addCase(publish.pending, (state) => {
        state.publishing = true;
        state.publishingError = undefined;
    })
    .addCase(publish.rejected, (state, action) => {
        state.publishing = false;
        state.publishingError = action.payload as ErrorDto;
    })
    .addCase(publish.fulfilled, (state) => {
        state.publishing = false;
        state.publishingError = undefined;
    }));
