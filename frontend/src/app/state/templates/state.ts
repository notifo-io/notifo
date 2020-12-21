/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, ListState } from '@app/framework';
import { TemplateDto } from '@app/service';

export interface TemplatesStateInStore {
    templates: TemplatesState;
}

export interface TemplatesState {
    // All templates.
    templates: ListState<TemplateDto>;

    // The current template code.
    currentTemplateCode?: string;

    // True if upserting.
    upserting?: boolean;

    // The creating error.
    upsertingError?: ErrorDto;
}
