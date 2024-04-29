/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { ChannelTemplateDetailsDtoOfMessagingTemplateDto, ChannelTemplateDto } from '@app/service';

export interface MessagingTemplatesStateInStore {
    messagingTemplates: MessagingTemplatesState;
}

export interface MessagingTemplatesState {
    // All templates.
    templates: ListState<ChannelTemplateDto>;

    // The template details.
    template?: ChannelTemplateDetailsDtoOfMessagingTemplateDto;

    // Mutation for loading the messaging templates.
    loadingTemplate?: MutationState;

    // Mutation for creating an messaging template.
    creating?: MutationState;

    // Mutation for updating an messaging template.
    updating?: MutationState;

    // Mutation for deleting an messaging template.
    deleting?: MutationState;
}
