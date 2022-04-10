/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { useField, useFormikContext } from 'formik';
import * as React from 'react';
import { Col, CustomInput, Row } from 'reactstrap';
import { FormEditorProps, Forms, isErrorVisible, Types } from '@app/framework';
import { texts } from '@app/texts';
import { CHANNELS, CONDITION_MODES, CONFIRM_MODES, SEND_MODES } from './../utils/model';
import { EmailTemplateInput } from './EmailTemplateInput';
import { MediaInput } from './MediaInput';
import { MessagingTemplateInput } from './MessagingTemplateInput';
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
                    label={texts.common.linkUrl} hints={texts.common.linkUrlHints}  />

                <Forms.LocalizedText name={`${field}.linkText`} {...props}
                    label={texts.common.linkText} hints={texts.common.linkTextHints}  />

                <Forms.LocalizedText name={`${field}.confirmText`} {...props}
                    label={texts.common.confirmText} hints={texts.common.confirmTextHints} />

                <Forms.Select name={`${field}.confirmMode`} {...props} options={CONFIRM_MODES}
                    label={texts.common.confirmMode} hints={texts.common.confirmModeHints} />
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
        const { field, disabled, vertical } = props;

        return (
            <>
                <fieldset disabled={disabled}>
                    <legend>{texts.common.rules}</legend>

                    <div className='rules'>
                        {CHANNELS.map(channel => (
                            <Channel key={channel} channel={channel} {...props} />
                        ))}
                    </div>
                </fieldset>

                <fieldset disabled={disabled}>
                    <legend>{texts.notificationSettings.email.title}</legend>
                    
                    <EmailTemplateInput name={`${field}.email.template`} vertical={vertical}
                        label={texts.common.template} hints={texts.notificationSettings.templateHints} />

                    <Forms.Email name={`${field}.email.properties.fromEmail`} vertical={vertical}
                        label={texts.common.fromEmail} hints={texts.notificationSettings.fromEmailHints} />

                    <Forms.Text name={`${field}.email.properties.fromName`} vertical={vertical}
                        label={texts.common.fromName} hints={texts.notificationSettings.fromNameHints} />
                </fieldset>

                <fieldset disabled={disabled}>
                    <legend>{texts.notificationSettings.messaging.title}</legend>

                    <MessagingTemplateInput name={`${field}.messaging.template`} vertical={vertical}
                        label={texts.common.template} hints={texts.notificationSettings.templateHints} />
                </fieldset>

                <fieldset disabled={disabled}>
                    <legend>{texts.notificationSettings.sms.title}</legend>

                    <SmsTemplateInput name={`${field}.sms.template`} vertical={vertical}
                        label={texts.common.template} hints={texts.notificationSettings.templateHints} />
                </fieldset>

                <fieldset disabled={disabled}>
                    <legend>{texts.notificationSettings.webhook.title}</legend>

                    <WebhookInput name={`${field}.webhook.template`} vertical={vertical}
                        label={texts.common.template} hints={texts.notificationSettings.webhookHints} />
                </fieldset>
            </>
        );
    };

    const Channel = (props: SettingsProps & { channel: string }) => {
        const { channel, field } = props;
    
        return (
            <Row noGutters>
                <Col className='rules-send'>
                    <Forms.Select name={`${field}.${channel}.send`} vertical options={SEND_MODES} />
                </Col>
                <Col className='rules-label' xs='auto'>
                    <small>{texts.common.via}</small>
                </Col>
                <Col className='rules-label rules-type'>
                    {texts.notificationSettings[channel].title}
                </Col>
                <Col className='rules-label' xs='auto'>
                    <small>{texts.common.after}</small>
                </Col>
                <Col className='rules-delay'>
                    <Forms.Number name={`${field}.${channel}.delayInSeconds`} vertical min={0} max={6000} />
                </Col>
                <Col className='rules-label' xs='auto'>
                    <small>{texts.common.secondsShort}</small>
                </Col>
                <Col className='rules-label' xs='auto'>
                    <small>{texts.common.when}</small>
                </Col>
                <Col className='rules-condition'>
                    <Forms.Select name={`${field}.${channel}.condition`} vertical options={CONDITION_MODES} />
                </Col>
            </Row>
        );
    };
}

export const InputSelect = ({ name, options }: FormEditorProps & { options: Forms.Option<string | number>[] }) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField<string | number>(name);

    return (
        <>
            <CustomInput type='select' name={field.name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            >
                {Types.isUndefined(field.value) && !options.find(x => x.value === field.value) &&
                    <option></option>
                }

                {options.map((option, i) =>
                    <option key={i} value={option.value}>{option.label}</option>,
                )}
            </CustomInput>
        </>
    );
};