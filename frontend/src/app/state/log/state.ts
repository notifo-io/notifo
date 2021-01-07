/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState } from '@app/framework';
import { LogEntryDto } from '@app/service';

export interface LogStateInStore {
    log: LogState;
}

export interface LogState {
    // All log entries.
    entries: ListState<LogEntryDto>;
}
