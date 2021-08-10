/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Forms } from '@app/framework';
import { texts } from '@app/texts';
import * as React from 'react';
import { CHANNELS, CONFIRM_MODES, SEND_MODES } from './../utils/model';
import { EmailTemplateInput } from './EmailTemplateInput';
import { MediaInput } from './MediaInput';
import { SmsTemplateInput } from './SmsTemplateInput';
import { WebhookInput } from './WebhookInput';

export module NotificationsForm {
    export interface FormattingProps {
        // The name of the field.
        field: string;

        // True when the form is disabled.
        disabled?: boolean;

        // The layout.
        vertical?: boolean;

        // The selected language.
        language: string;

        // The languages.
        languages: ReadonlyArray<string>;

        // Triggered when the language is selected.
        onLanguageSelect: (language: string) => void;
    }

    export const Formatting = (props: FormattingProps) => {
        const { disabled, field } = props;

        return (
            <fieldset disabled={disabled}>
                <Forms.LocalizedText name={`${field}.subject`} {...props}
                    label={texts.common.messageSubject} />

                <Forms.LocalizedTextArea name={`${field}.body`} {...props}
                    label={texts.common.messageBody} />

                <MediaInput name={`${field}.imageSmall`} {...props}
                    label={texts.common.imageSmall} />

                <MediaInput name={`${field}.imageLarge`} {...props}
                    label={texts.common.imageLarge} />

                <Forms.LocalizedText name={`${field}.linkUrl`} {...props}
                    label={texts.common.linkUrl} />

                <Forms.LocalizedText name={`${field}.linkText`} {...props}
                    label={texts.common.linkText} />

                <Forms.LocalizedText name={`${field}.confirmText`} {...props}
                    label={texts.common.confirmText} />

                <Forms.Select name={`${field}.confirmMode`} {...props} options={CONFIRM_MODES}
                    label={texts.common.confirmMode} />
            </fieldset>
        );
    };

    export interface SettingsProps {
        // The name of the field.
        field: string;

        // True when the form is disabled.
        disabled?: boolean;

        // The layout.
        vertical?: boolean;
    }

    export const Settings = (props: SettingsProps) => {
        return (
            <>
                {CHANNELS.map(channel => (
                    <Channel key={channel} channel={channel} {...props} />
                ))}
            </>
        );
    };

    const Channel = ({ channel, disabled, field, vertical }: { disabled?: boolean; vertical?: boolean; field: string; channel: string }) => {
        return (
            <fieldset key={channel} disabled={disabled}>
                <legend>{texts.notificationSettings[channel].title}</legend>

                {channel !== 'webhook' &&
                    <Forms.Select name={`${field}.${channel}.send`} vertical={vertical} options={SEND_MODES}
                        label={texts.common.send} />
                }

                <Forms.Number name={`${field}.${channel}.delayInSeconds`} vertical={vertical} min={0} max={6000}
                    label={texts.notificationSettings.delayInSeconds} />

                {channel === 'sms' &&
                    <SmsTemplateInput name={`${field}.${channel}.template`} vertical={vertical}
                        label={texts.common.template} />
                }

                {channel === 'email' &&
                    <EmailTemplateInput name={`${field}.${channel}.template`} vertical={vertical}
                        label={texts.common.template} />
                }

                {channel === 'webhook' &&
                    <WebhookInput name={`${field}.${channel}.template`} vertical={vertical}
                        label={texts.common.template} />
                }
            </fieldset>
        );
    };
}
