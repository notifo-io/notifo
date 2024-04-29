/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { ChannelTemplateDetailsDtoOfEmailTemplateDto, ChannelTemplateDto, MjmlSchema } from '@app/service';

export interface EmailTemplatesStateInStore {
    emailTemplates: EmailTemplatesState;
}

export interface EmailTemplatesState {
    // The schema.
    schema?: MjmlSchema;

    // All templates.
    templates: ListState<ChannelTemplateDto>;

    // The template details.
    template?: ChannelTemplateDetailsDtoOfEmailTemplateDto;

    // Mutation for loading the email templates.
    loadingTemplate?: MutationState;

    // Mutation for creating an email template.
    creating?: MutationState;

    // Mutation for creating an email template language.
    creatingLanguage?: MutationState;

    // Mutation for updating an email template.
    updating?: MutationState;

    // Mutation for updating an email template language.
    updatingLanguage?: MutationState;

    // Mutation for deleting an email template.
    deleting?: MutationState;

    // Mutation for deleting an email template language.
    deletingLanguage?: MutationState;
}
