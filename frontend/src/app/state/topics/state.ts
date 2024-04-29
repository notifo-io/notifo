/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { TopicDto } from '@app/service';

export interface TopicsStateInStore {
    topics: TopicsState;
}

export interface TopicsState {
    // All topics.
    topics: ListState<TopicDto>;
    
    // Mutation for upserting.
    upserting?: MutationState;
}
