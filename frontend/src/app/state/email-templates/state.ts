/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo, ListState } from '@app/framework';
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

    // True if loading the email templates.
    loadingTemplate?: boolean;

    // The email templates loading error.
    loadingTemplateError?: ErrorInfo;

    // True if creating an email template.
    creating?: boolean;

    // The error if creating an email template fails.
    creatingError?: ErrorInfo;

    // True if creating an email template language.
    creatingLanguage?: boolean;

    // The error if creating an email template language fails.
    creatingLanguageError?: ErrorInfo;

    // True if updating an email template.
    updating?: boolean;

    // The error if updating an email template fails.
    updatingError?: ErrorInfo;

    // True if updating an email template language.
    updatingLanguage?: boolean;

    // The error if updating an email template language fails.
    updatingLanguageError?: ErrorInfo;

    // True if deleting an email template.
    deleting?: boolean;

    // The error if deleting an email template language.
    deletingError?: ErrorInfo;

    // True if deleting an email template language.
    deletingLanguage?: boolean;

    // The error if deleting an email template language fails.
    deletingLanguageError?: ErrorInfo;
}
