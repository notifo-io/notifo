/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListState, MutationState } from '@app/framework';
import { ChannelTemplateDetailsDtoOfSmsTemplateDto, ChannelTemplateDto } from '@app/service';

export interface SmsTemplatesStateInStore {
    smsTemplates: SmsTemplatesState;
}

export interface SmsTemplatesState {
    // All templates.
    templates: ListState<ChannelTemplateDto>;

    // The template details.
    template?: ChannelTemplateDetailsDtoOfSmsTemplateDto;

    // Mutation for loading the Sms templates.
    loadingTemplate?: MutationState;

    // Mutation for creating an Sms template.
    creating?: MutationState;

    // Mutation for updating an Sms template.
    updating?: MutationState;

    // Mutation for deleting an Sms template.
    deleting?: MutationState;
}
