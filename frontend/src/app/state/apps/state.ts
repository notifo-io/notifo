/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework/model';
import { AppDetailsDto, AppDto, AuthSchemeResponseDto } from '@app/service';

export interface AppsStateInStore {
    apps: AppsState;
}

export interface CreateAppParams {
    // The name of the app.
    name: string;
}

export interface AppsState {
    apps: ListState<AppDto>;

    // The selected app.
    appId?: string;

    // The app details.
    app?: AppDetailsDto;

    // The auth details.
    auth?: AuthSchemeResponseDto;

    // Mutation for loading details.
    loadingDetails?: MutationState;

    // Mutation for creating.
    creating?: MutationState;

    // Mutation for updating the app.
    updating?: MutationState;

    // Mutation for updating the app.
    updatingContributors?: MutationState;

    // Mutation for updating the auth settings.
    updatingAuth?: MutationState;
}
