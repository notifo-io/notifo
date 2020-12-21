/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Forms } from '@app/framework';
import { texts } from '@app/texts';
import * as React from 'react';

const CHANNELS = [
    'webpush',
    'mobilepush',
    'email',
    'sms',
];

const CONFIRM_MODES = [{
        value: 'Seen',
        label: texts.common.confirmModes.seen,
    }, {
        value: 'Explicit',
        label: texts.common.confirmModes.explicit,
    }, {
        value: 'None',
        label: texts.common.confirmModes.none,
    },
];

export module NotificationsForm {
    export interface FormattingProps {
        // The name of the field.
        field: string;

        // True when the form is disabled.
        disabled?: boolean;

        // The selected language.
        language: string;

        // The languages.
        languages: ReadonlyArray<string>;

        // Triggered when the language is selected.
        onLanguageSelect: (language: string) => void;
    }

    export const Formatting = (props: FormattingProps) => {
        const { disabled, field, language, languages, onLanguageSelect } = props;

        const currentLanguage = language || languages[0];

        return (
            <fieldset disabled={disabled}>
                <Forms.LocalizedText name={`${field}.subject`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.messageSubject} />

                <Forms.LocalizedTextArea name={`${field}.body`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.messageBody} />

                <Forms.LocalizedText name={`${field}.imageSmall`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.imageSmall} />

                <Forms.LocalizedText name={`${field}.imageLarge`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.imageLarge} />

                <Forms.LocalizedText name={`${field}.linkUrl`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.linkUrl} />

                <Forms.LocalizedText name={`${field}.linkText`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.linkText} />

                <Forms.LocalizedText name={`${field}.confirmText`}
                    onLanguageSelect={onLanguageSelect}
                    languages={languages}
                    language={currentLanguage}
                    label={texts.common.confirmText} />

                <Forms.Select name={`${field}.confirmMode`} options={CONFIRM_MODES}
                    label={texts.common.confirmMode} />
            </fieldset>
        );
    };

    export interface SettingsProps {
        // The name of the field.
        field: string;

        // True when the form is disabled.
        disabled?: boolean;
    }

    export const Settings = (props: SettingsProps) => {
        const { disabled, field } = props;

        return (
            <>
                {CHANNELS.map(channel => (
                    <Channel key={channel} channel={channel} disabled={disabled} field={field} />
                ))}
            </>
        );
    };

    const Channel = ({ channel, disabled, field }: { disabled?: boolean, field: string, channel: string }) => {
        return (
            <fieldset key={channel}>
                <legend>{texts.notificationSettings[channel].title}</legend>

                <Forms.Boolean name={`${field}.${channel}.send`} disabled={disabled} indeterminate
                    label={texts.notificationSettings[channel].send} />

                <Forms.Number name={`${field}.${channel}.delayInSeconds`}
                    label={texts.notificationSettings.delayInSeconds} min={0} max={6000} />
            </fieldset>
        );
    };
}
