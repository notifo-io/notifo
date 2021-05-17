/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

type Option<T> = { value: T | string; label: string };

export interface CoreStateInStore {
    core: CoreState;
}

export interface CoreState {
    // All timezones.
    timezones: Option<string>[];

    // All languages.
    languages: Option<string>[];
}
