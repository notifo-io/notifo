/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo, ListState } from '@app/framework';
import { SubscriptionDto } from '@app/service';

export interface SubscriptionsStateInStore {
    subscriptions: SubscriptionsState;
}

export interface SubscriptionsState {
    // All subscriptions.
    subscriptions: ListState<SubscriptionDto>;

    // True if upserting.
    upserting?: boolean;

    // The creating error.
    upsertingError?: ErrorInfo;
}
