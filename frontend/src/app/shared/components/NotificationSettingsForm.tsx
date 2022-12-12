/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { useController, useFormContext } from 'react-hook-form';
import { Button, Card, CardBody, CardHeader, Col, Row } from 'reactstrap';
import { FormAlert, Icon, Toggle, Types, useEventCallback } from '@app/framework';
import { texts } from '@app/texts';
import { CHANNELS, CONDITION_MODES, CONFIRM_MODES, REQUIRED_MODES, SCHEDULING_TYPES, SEND_MODES, WEEK_DAYS } from './../utils/model';
import { EmailTemplateInput } from './EmailTemplateInput';
import { Forms } from './Forms';
import { MessagingTemplateInput } from './MessagingTemplateInput';
import { SmsTemplateInput } from './SmsTemplateInput';
import { WebhookInput } from './WebhookInput';

const PICK_MEDIA = {
    pickMedia: true,
    pickArgument: true,
};

const PICK_TEXT = {
    pickEmoji: true,
    pickArgument: true,
};

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
                <Forms.LocalizedText name={`${field}.subject`} {...props} picker={PICK_TEXT}
                    label={texts.common.messageSubject} />

                <Forms.LocalizedTextArea name={`${field}.body`} {...props} picker={PICK_TEXT}
                    label={texts.common.messageBody} />

                <Forms.LocalizedText name={`${field}.imageSmall`} {...props} picker={PICK_MEDIA}
                    label={texts.common.imageSmall} />

                <Forms.LocalizedText name={`${field}.imageLarge`} {...props} picker={PICK_MEDIA}
                    label={texts.common.imageLarge} />

                <Forms.LocalizedText name={`${field}.linkUrl`} {...props} picker={PICK_TEXT}
                    label={texts.common.linkUrl} hints={texts.common.linkUrlHints}  />

                <Forms.LocalizedText name={`${field}.linkText`} {...props} picker={PICK_TEXT}
                    label={texts.common.linkText} hints={texts.common.linkTextHints}  />

                <Forms.LocalizedText name={`${field}.confirmText`} {...props} picker={PICK_TEXT}
                    label={texts.common.confirmText} hints={texts.common.confirmTextHints} />

                <Forms.Select name={`${field}.confirmMode`} {...props} options={CONFIRM_MODES}
                    label={texts.common.confirmMode} hints={texts.common.confirmModeHints} />
            </fieldset>
        );
    };

    export interface SchedulingProps {
        // The name of the field.
        field: string;

        // True when the form is disabled.
        disabled?: boolean;

        // The layout.
        vertical?: boolean;
    }

    export const Scheduling = (props: SettingsProps) => {
        const { disabled, field } = props;
        const { watch } = useFormContext();

        return (
            <>
                <fieldset disabled={disabled}>
                    <legend>{texts.notificationSettings.scheduling}</legend>

                    <SchedulingToggle name={field} />

                    {Types.isObject(watch(field)) &&
                        <div className='pt-4'>
                            <FormAlert text={texts.notificationSettings.schedulingInfo} />

                            <Forms.Select name={`${field}.type`} options={SCHEDULING_TYPES}
                                label={texts.common.mode} />
        
                            <Forms.Select name={`${field}.nextWeekDay`} options={WEEK_DAYS}
                                label={texts.common.weekDay} />
        
                            <Forms.Date name={`${field}.date`}
                                label={texts.common.date} />
        
                            <Forms.Time name={`${field}.time`}
                                label={texts.common.timeOfDay} />
                        </div>
                    }
                </fieldset>
            </>
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
        const { disabled } = props;

        return (
            <>
                <FormAlert text={texts.notificationSettings.settingsInfo} />

                <fieldset disabled={disabled}>
                    <legend>{texts.common.rules}</legend>

                    <div className='rules'>
                        {CHANNELS.map(channel => (
                            <Channel key={channel} channel={channel} {...props} />
                        ))}
                    </div>
                </fieldset>
            </>
        );
    };

    const Channel = (props: SettingsProps & { channel: string }) => {
        const { channel, field, vertical } = props;
        const { getValues, setValue } = useFormContext();
        const fieldSend = `${field}.${channel}.send`;
        const fieldCondition = `${field}.${channel}.condition`;
        const [isExpanded, setIsExpanded] = React.useState(false);
        
        React.useEffect(() => {
            if (!getValues(fieldSend)) {
                setValue(fieldSend, SEND_MODES[0].value);
            }
        }, [fieldSend, getValues, setValue]);
        
        React.useEffect(() => {
            if (!getValues(fieldCondition)) {
                setValue(fieldCondition, SEND_MODES[0].value);
            }
        }, [fieldCondition, getValues, setValue]);

        const doToggle = useEventCallback(() => {
            setIsExpanded(x => !x);
        });
    
        return (
            <Card>
                <CardHeader className={classNames({ collapsed: !isExpanded })}>
                    <Row className='align-items-center' noGutters>
                        <Col className='rules-expand'>
                            <Button size='sm' color='link' onClick={doToggle}>
                                <Icon type={isExpanded ? 'expand_less' : 'expand_more'} />
                            </Button>
                        </Col>
                        <Col className='rules-send'>
                            <Forms.Select name={fieldSend} vertical options={SEND_MODES} />
                        </Col>
                        <Col className='rules-label' xs='auto'>
                            {texts.common.via}
                        </Col>
                        <Col>
                            {texts.notificationSettings[channel].title}
                        </Col>
                        <Col className='rules-label' xs='auto'>
                            {texts.common.when}
                        </Col>
                        <Col className='rules-condition'>
                            <Forms.Select name={fieldCondition} vertical options={CONDITION_MODES} />
                        </Col>
                    </Row>
                </CardHeader>

                {isExpanded &&
                    <CardBody>
                        <Forms.Number name={`${field}.${channel}.delayInSeconds`} min={0} max={6000} vertical={vertical}
                            label={texts.notificationSettings.delayInSeconds} hints={texts.notificationSettings.delayInSecondsHints} />
                            
                        <Forms.Select name={`${field}.${channel}.required`} options={REQUIRED_MODES} vertical={vertical}
                            label={texts.notificationSettings.required} hints={texts.notificationSettings.requiredHints} />

                        {channel === 'email' &&
                            <>
                                <hr />

                                <EmailTemplateInput name={`${field}.${channel}.template`} vertical={vertical}
                                    label={texts.common.template} hints={texts.notificationSettings.templateHints} />

                                <Forms.Email name={`${field}.${channel}.properties.fromEmail`} vertical={vertical}
                                    label={texts.common.fromEmail} hints={texts.notificationSettings.fromEmailHints} />

                                <Forms.Text name={`${field}.${channel}.properties.fromName`} vertical={vertical}
                                    label={texts.common.fromName} hints={texts.notificationSettings.fromNameHints} />
                            </>
                        }

                        {channel === 'messaging' &&
                            <>            
                                <hr />

                                <MessagingTemplateInput name={`${field}.${channel}.template`} vertical={vertical}
                                    label={texts.common.template} hints={texts.notificationSettings.templateHints} />
                            </>
                        }

                        {channel === 'sms' &&
                            <>           
                                <hr />

                                <SmsTemplateInput name={`${field}.${channel}.template`} vertical={vertical}
                                    label={texts.common.template} hints={texts.notificationSettings.templateHints} />
                            </>
                        }

                        {channel === 'webhook' &&
                            <>
                                <hr />

                                <WebhookInput name={`${field}.${channel}.template`} vertical={vertical}
                                    label={texts.common.template} hints={texts.notificationSettings.webhookHints} />
                            </>
                        }
                    </CardBody>
                }
            </Card>
        );
    };
}

const SchedulingToggle = ({ name }: { name: string }) => {
    const { setValue } = useFormContext();
    const { field } = useController({ name });

    const doToggle = useEventCallback((value: any) => {
        if (value) {
            field.onChange({ type: 'UTC', time: '00:00' });
        } else {
            field.onChange(undefined);
        }

        setValue('hasScheduling', true);
    });

    return (
        <>
            <Toggle value={Types.isObject(field.value)} onChange={doToggle} label={texts.notificationSettings.schedulingToggle} />
        </>
    );
};