/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createAction } from '@reduxjs/toolkit';
import { createExtendedReducer, createMutation } from '@app/framework';
import { Clients, PublishDto } from '@app/service';
import { PublishState } from './state';

export const togglePublishDialog = createAction<{ open: boolean; values?: Partial<PublishDto> }>('publish/dialog');

export const publish = createMutation<PublishState>('publishing').with({
    name: 'publish/publish',
    mutateFn: async (arg: { appId: string; params: PublishDto }) => {
        await Clients.Events.postEvents(arg.appId, { requests: [arg.params] });
    },
});

const initialState: PublishState = {};

const operations = [
    publish,
];

export const publishReducer = createExtendedReducer(initialState, builder => builder
    .addCase(togglePublishDialog, (state, action) => {
        state.dialogOpen = action.payload.open;
        state.dialogValues = action.payload?.values;
        state.publishing = {};
    }),
operations);
