/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { SubscriptionDto } from '@app/service';

export interface SubscriptionsStateInStore {
    subscriptions: SubscriptionsState;
}

export interface SubscriptionsState {
    // All subscriptions.
    subscriptions: ListState<SubscriptionDto>;

    // Mutation for upserting.
    upserting?: MutationState;
}
