/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { ListState } from '@app/framework/model';
import { AppDetailsDto, AppDto } from '@app/service';

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

    // True if loading details.
    loadingDetails?: boolean;

    // The loading details error.
    loadingDetailsError?: ErrorDto;

    // True if creating.
    creating?: boolean;

    // The creating error.
    creatingError?: ErrorDto;

    // True if upserting.
    upserting?: boolean;

    // The upserting error.
    upsertingError?: ErrorDto;

    // True if updating the contributors.
    contributorsUpdating?: boolean;

    // The contributor error.
    contributorsError?: ErrorDto;
}

export function getApp(state: AppsState) {
    return state.apps.items?.find(x => x.id === state.appId)!;
}
