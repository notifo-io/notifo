/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { useFormikContext } from 'formik';
import * as React from 'react';
import { Button, Input } from 'reactstrap';
import { Forms, Icon, isErrorVisible, LocalizedFormProps, useFieldNew } from '@app/framework';
import { LanguageSelector } from '@app/framework/react/LanguageSelector';
import { MediaPicker } from './MediaPicker';

export const MediaInput = (props: LocalizedFormProps) => {
    const {
        className,
        language,
        languages,
        name,
        onLanguageSelect,
        ...other
    } = props;

    const fieldName = `${props.name}.${props.language}`;

    const [isPickerOpen, setIsPickerOpen] = React.useState(false);

    const { submitCount, setFieldValue } = useFormikContext();
    const [field, meta] = useFieldNew(fieldName);

    const doSelectUrl = (url: string) => {
        setFieldValue(fieldName, url);
        setIsPickerOpen(false);
    };

    const doClose = () => {
        setIsPickerOpen(false);
    };

    return (
        <Forms.Row className='localized-value' {...props}>
            <div className='input-container localized-input'>
                <div className='localized-languages'>
                    <LanguageSelector
                        languages={languages}
                        language={language}
                        onSelect={onLanguageSelect} />
                </div>

                <Input type='text' name={field.name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    {...other}
                />

                <Button color='link-secondary' className='input-btn' tabIndex={-1} onClick={() => setIsPickerOpen(true)}>
                    <Icon type='photo_size_select_actual' />
                </Button>
            </div>

            {isPickerOpen &&
                <MediaPicker selectedUrl={field.value}
                    onClose={doClose}
                    onSelected={doSelectUrl}
                />
            }
        </Forms.Row>
    );
};
