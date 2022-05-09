/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo, ListState } from '@app/framework';
import { TopicDto } from '@app/service';

export interface TopicsStateInStore {
    topics: TopicsState;
}

export interface TopicsState {
    // All topics.
    topics: ListState<TopicDto>;
    
    // True if upserting.
    upserting?: boolean;

    // The error if upserting fails.
    upsertingError?: ErrorInfo;
}
