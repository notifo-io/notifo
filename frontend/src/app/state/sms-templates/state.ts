/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo, ListState } from '@app/framework';
import { ChannelTemplateDetailsDtoOfSmsTemplateDto, ChannelTemplateDto } from '@app/service';

export interface SmsTemplatesStateInStore {
    smsTemplates: SmsTemplatesState;
}

export interface SmsTemplatesState {
    // All templates.
    templates: ListState<ChannelTemplateDto>;

    // The template details.
    template?: ChannelTemplateDetailsDtoOfSmsTemplateDto;

    // True if loading the Sms templates.
    loadingTemplate?: boolean;

    // The Sms templates loading error.
    loadingTemplateError?: ErrorInfo;

    // True if creating an Sms template.
    creating?: boolean;

    // The error if creating an Sms template fails.
    creatingError?: ErrorInfo;

    // True if updating an Sms template.
    updating?: boolean;

    // The error if updating an Sms template fails.
    updatingError?: ErrorInfo;

    // True if deleting an Sms template.
    deleting?: boolean;

    // The error if deleting an Sms template language.
    deletingError?: ErrorInfo;
}
