import { FieldArray } from 'formik';
import * as React from 'react';
import { Button, Col, Row } from 'reactstrap';
import { FormEditorProps, Forms, Icon } from '@app/framework';
import { TemplateDto } from '@app/service';
import { texts } from '@app/texts';
import { TemplateInput } from './TemplateInput';

type TemplateVariants = { probability: number; templateCode: string }[];

export interface TemplateVariantsInputProps extends FormEditorProps {
    // The variants to render.
    variants: TemplateVariants | undefined;
    
    // The actual templates.
    templates: ReadonlyArray<TemplateDto> | undefined;
}

export const TemplateVariantsInput = (props: TemplateVariantsInputProps) => {
    const { name, templates, variants, ...other } = props;

    return (
        <Forms.Row name={name} {...other} hideError>
            <FieldArray name={name} render={arrayHelper =>
                <div>
                    {variants?.map((_, i) =>
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
                                <Button color='danger' onClick={() => arrayHelper.remove(i)}>
                                    <Icon type='delete' />
                                </Button>
                            </Col>
                        </Row>,
                    )}
                
                    <Button color='success' onClick={() => arrayHelper.push({})}>
                        <Icon type='add' />
                    </Button>
                </div>
            } />
        </Forms.Row>
    );
};