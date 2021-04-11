/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { ConfiguredIntegrationsDto } from '@app/service';

export interface IntegrationsStateInStore {
    integrations: IntegrationsState;
}

export interface IntegrationsState extends ConfiguredIntegrationsDto {
    // True if loading integrations.
    loading?: boolean;

    // The loading integrations error.
    loadingError?: ErrorDto;

    // True if upserting integrations.
    upserting?: boolean;

    // The upserting integrations error.
    upsertingError?: ErrorDto;
}
