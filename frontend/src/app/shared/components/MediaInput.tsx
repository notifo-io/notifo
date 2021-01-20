/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Forms, Icon, isErrorVisible, LocalizedFormProps } from '@app/framework';
import { LanguageSelector } from '@app/framework/react/LanguageSelector';
import { useField, useFormikContext } from 'formik';
import * as React from 'react';
import { Button, Col, FormGroup, Input, Label, Row } from 'reactstrap';
import { MediaPicker } from './MediaPicker';

export const MediaInput = (props: LocalizedFormProps) => {
    const {
        className,
        label,
        language,
        languages,
        name,
        onLanguageSelect,
        ...other
    } = props;

    const fieldName = `${props.name}.${props.language}`;

    const [isPickerOpen, setIsPickerOpen] = React.useState(false);

    const { submitCount, setFieldValue } = useFormikContext();
    const [field, meta] = useField(fieldName);

    const doSelectUrl = (url: string) => {
        setFieldValue(fieldName, url);
        setIsPickerOpen(false);
    };

    const doClose = () => {
        setIsPickerOpen(false);
    };

    return (
        <FormGroup className={className}>
            <Row>
                <Col>
                    <Label for={name}>{label}</Label>
                </Col>
                <Col xs='auto'>
                    <LanguageSelector
                        languages={languages}
                        language={language}
                        onSelect={onLanguageSelect} />
                </Col>
            </Row>

            <Forms.Error name={name} />

            <div className='input-container'>

                <Input type='text' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
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
        </FormGroup>
    );
};
