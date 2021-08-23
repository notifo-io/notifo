/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, ListState } from '@app/framework';
import { ChannelTemplateDetailsDtoOfMessagingTemplateDto, ChannelTemplateDto } from '@app/service';

export interface MessagingTemplatesStateInStore {
    messagingTemplates: MessagingTemplatesState;
}

export interface MessagingTemplatesState {
    // All templates.
    templates: ListState<ChannelTemplateDto>;

    // The template details.
    template?: ChannelTemplateDetailsDtoOfMessagingTemplateDto;

    // True if loading the messaging templates.
    loadingTemplate?: boolean;

    // The messaging templates loading error.
    loadingTemplateError?: ErrorDto;

    // True if creating an messaging template.
    creating?: boolean;

    // The error if creating an messaging template fails.
    creatingError?: ErrorDto;

    // True if updating an messaging template.
    updating?: boolean;

    // The error if updating an messaging template fails.
    updatingError?: ErrorDto;

    // True if deleting an messaging template.
    deleting?: boolean;

    // The error if deleting an messaging template language.
    deletingError?: ErrorDto;
}
