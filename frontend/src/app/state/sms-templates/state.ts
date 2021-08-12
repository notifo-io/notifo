/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, ListState } from '@app/framework';
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
    loadingTemplateError?: ErrorDto;

    // True if creating an Sms template.
    creating?: boolean;

    // The error if creating an Sms template fails.
    creatingError?: ErrorDto;

    // True if updating an Sms template.
    updating?: boolean;

    // The error if updating an Sms template fails.
    updatingError?: ErrorDto;

    // True if deleting an Sms template.
    deleting?: boolean;

    // The error if deleting an Sms template language.
    deletingError?: ErrorDto;
}
