/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { TemplateDto } from '@app/service';

export interface TemplatesStateInStore {
    templates: TemplatesState;
}

export interface TemplatesState {
    // All templates.
    templates: ListState<TemplateDto>;

    // The current template code.
    currentTemplateCode?: string;

    // Mutation for upserting.
    upserting?: MutationState;
}
