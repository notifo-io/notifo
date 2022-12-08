/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useFieldArray } from 'react-hook-form';
import { Button, Col, Row } from 'reactstrap';
import { Icon } from '@app/framework';
import { TemplateDto } from '@app/service';
import { texts } from '@app/texts';
import { FormEditorProps, Forms } from './Forms';
import { TemplateInput } from './TemplateInput';

export interface TemplateVariantsInputProps extends FormEditorProps {    
    // The actual templates.
    templates: ReadonlyArray<TemplateDto> | undefined;
}

export const TemplateVariantsInput = (props: TemplateVariantsInputProps) => {
    const { name, templates, ...other } = props;
    const { fields, append, remove } = useFieldArray({ name });

    return (
        <Forms.Row name={name} {...other} hideError>                
            <div>
                {fields?.map((_, i) =>
                    <Row noGutters key={i}>
                        <Col className='variant-code'>
                            <TemplateInput name={`${name}[${i}].templateCode`} vertical templates={templates} />
                        </Col>
                        <Col className='variant-label' xs='auto'>
                            <small>{texts.common.with}</small>
                        </Col>
                        <Col className='variant-probability' xs={4}>
                            <Forms.Number name={`${name}[${i}].probability`} vertical min={0} max={100} />
                        </Col>
                        <Col className='variant-label' xs='auto'>
                            <small>%</small>
                        </Col>
                        <Col xs='auto'>
                            <Button color='danger' onClick={() => remove(i)}>
                                <Icon type='delete' />
                            </Button>
                        </Col>
                    </Row>,
                )}
            
                <Button color='success' onClick={() => append({})}>
                    <Icon type='add' />
                </Button>
            </div>
        </Forms.Row>
    );
};