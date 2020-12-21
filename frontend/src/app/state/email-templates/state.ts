/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { EmailTemplateDto } from '@app/service';

export interface EmailTemplatesStateInStore {
    emailTemplates: EmailTemplatesState;
}

export interface EmailTemplatesState {
    // The email templates.
    emailTemplates: { [language: string]: EmailTemplateDto };

    // True if loading the email templates.
    loading?: boolean;

    // The email templates loading error.
    loadingError?: ErrorDto;

    // True if updating the email template.
    creating?: boolean;

    // The email template creating error.
    creatingError?: ErrorDto;

    // True if updating the email template.
    updating?: boolean;

    // The email template updating error.
    updatingError?: ErrorDto;

    // True if deleting the email template.
    deleting?: boolean;

    // The email template deleting error.
    deletingError?: ErrorDto;
}
