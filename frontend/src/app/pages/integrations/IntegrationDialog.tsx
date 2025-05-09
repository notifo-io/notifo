/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */


import { yupResolver } from '@hookform/resolvers/yup';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { Badge, Button, Col, Form, Label, Modal, ModalBody, ModalFooter, ModalHeader, Row } from 'reactstrap';
import * as Yup from 'yup';
import { Confirm, FormError, Icon, Loader, useEventCallback } from '@app/framework';
import { ConfiguredIntegrationDto, IntegrationDefinitionDto, IntegrationPropertyDto, UpdateIntegrationDto } from '@app/service';
import { Forms } from '@app/shared/components';
import { createIntegration, deleteIntegration, updateIntegration, useIntegrations } from '@app/state';
import { texts } from '@app/texts';
import { IntegrationImage } from './IntegrationImage';
import { StatusLabel } from './StatusLabel';

export interface IntegrationDialogProps {
    // The app id.
    appId: string;

    // The type code of the integration.
    type: string;

    // The definition.
    definition: IntegrationDefinitionDto;

    // The id of the integration.
    configuredId?: string;

    // The configured integration.
    configured?: ConfiguredIntegrationDto;

    // Invoked when closed.
    onClose: () => void;
}

export const IntegrationDialog = (props: IntegrationDialogProps) => {
    const {
        appId,
        configured,
        configuredId,
        definition,
        onClose,
        type,
    } = props;

    const dispatch = useDispatch<any>();
    const upserting = useIntegrations(x => x.upserting?.isRunning);
    const upsertingError = useIntegrations(x => x.upserting?.error);
    const wasUpserting = React.useRef(false);

    React.useEffect(() => {
        if (upserting) {
            wasUpserting.current = true;
        } else if (wasUpserting.current && !upsertingError) {
            onClose();
        }
    }, [upserting, upsertingError, onClose]);

    const doDelete = useEventCallback(() => {
        if (configuredId) {
            dispatch(deleteIntegration({ appId, id: configuredId }));

            onClose();
        }
    });

    const doUpsert = useEventCallback((params: UpdateIntegrationDto) => {
        for (const key of Object.keys(params.properties)) {
            params.properties[key] = params.properties[key]?.toString();
        }

        if (configuredId) {
            dispatch(updateIntegration({ appId, id: configuredId, params }));
        } else {
            dispatch(createIntegration({ appId, params: { type, ...params } }));
        }
    });

    const schema = React.useMemo(() => {
        const properties: { [name: string]: any } = {};

        for (const property of definition.properties) {
            const label = property.editorLabel || property.name;

            if (property.type === 'Number') {
                let propertyType: Yup.NumberSchema<number | undefined | null> = Yup.number().label(label);

                if (property.isRequired) {
                    propertyType = propertyType.requiredI18n();
                } else {
                    propertyType = propertyType.nullable();
                }

                if (property.minValue) {
                    propertyType = propertyType.min(property.minValue, texts.validation.minFn);
                }

                if (property.maxValue) {
                    propertyType = propertyType.max(property.maxValue, texts.validation.maxFn);
                }

                properties[property.name] = propertyType;
            } else if (property.type !== 'Boolean') {
                let propertyType: Yup.StringSchema<string | undefined | null> = Yup.string().label(label);

                if (property.isRequired) {
                    propertyType = propertyType.requiredI18n();
                } else {
                    propertyType = propertyType.nullable();
                }

                if (property.minLength) {
                    propertyType = propertyType.min(property.minLength, texts.validation.minLengthFn);
                }

                if (property.maxLength) {
                    propertyType = propertyType.max(property.maxLength, texts.validation.maxLengthFn);
                }

                if (property.format && property.format !== 'None') {
                    switch (property.format) {
                        case 'Email':
                            propertyType = propertyType.emailI18n();
                            break;
                        case 'HttpUrl':
                            propertyType = propertyType.httpUrlI18n();
                            break;
                    }
                }

                if (property.pattern) {
                    propertyType = propertyType.matches(new RegExp(property.pattern), texts.validation.patternFn);
                }

                properties[property.name] = propertyType;
            }
        }

        return Yup.object().shape({
            properties: Yup.object(properties),
        });
    }, [definition]);

    const initialValues = React.useMemo(() => {
        if (configured) {
            return {
                ...configured,
                properties: {
                    ...configured.properties,
                },
            };
        } else {
            const properties: Record<string, any> = {};

            for (const property of definition.properties) {
                properties[property.name] = property.defaultValue;
            }

            return {
                enabled: true,
                priority: 0,
                properties,
            };
        }
    }, [configured, definition.properties]);

    const form = useForm<UpdateIntegrationDto>({ resolver: yupResolver<any>(schema), defaultValues: initialValues, mode: 'onChange' });

    return (
        <Modal isOpen={true} size='lg' className='integration-dialog' backdrop={false} toggle={onClose}>
            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doUpsert)}>
                    <ModalHeader toggle={onClose}>
                        <Row noGutters>
                            <Col xs='auto' className='col-image'>
                                <IntegrationImage  definition={definition} type={type} />
                            </Col>

                            <Col>
                                <h4>{definition.title}</h4>

                                <div className='badges'>
                                    {definition.capabilities.map(capability => (
                                        <Badge key={capability} className='mr-1' color='secondary' pill>{capability}</Badge>
                                    ))}
                                </div>
                            </Col>
                        </Row>
                    </ModalHeader>

                    <ModalBody>
                        <fieldset className='mt-3' disabled={upserting}>
                            {configured &&
                                <Row className='mb-4'>
                                    <Col sm={4}>
                                        {texts.common.status}
                                    </Col>

                                    <Col sm={8}>
                                        <StatusLabel status={configured.status} />
                                    </Col>
                                </Row>
                            }

                            <Forms.Text name='condition' placeholder='properties.myProp == 1'
                                label={texts.integrations.condition} hints={texts.integrations.conditionHints} />

                            <Forms.Boolean name='test'
                                label={texts.integrations.test} hints={texts.integrations.testHints} indeterminate />

                            <Forms.Boolean name='enabled'
                                label={texts.common.enabled} hints={texts.integrations.enabledHints} />

                            <Forms.Number name='priority'
                                label={texts.integrations.priority} hints={texts.integrations.priorityHints} />

                            {definition.properties.length > 0 &&
                                <hr />
                            }

                            {definition.properties.map(property => (
                                <FormField key={property.name} property={property} />
                            ))}

                            {configuredId &&
                                <div>
                                    <hr />

                                    <Row>
                                        <Label sm={4} className='text-danger'>
                                            {texts.common.dangerZone}
                                        </Label>

                                        <Col sm={8}>
                                            <Confirm onConfirm={doDelete} text={texts.templates.confirmDelete}>
                                                {({ onClick }) => (
                                                    <Button color='danger' onClick={onClick} data-tooltip-id="default-tooltip" data-tooltip-content={texts.common.delete}>
                                                        <Icon type='delete' /> {texts.common.delete}
                                                    </Button>
                                                )}
                                            </Confirm>
                                        </Col>
                                    </Row>
                                </div>
                            }
                        </fieldset>

                        <FormError className='mt-4 mb-0' error={upsertingError} />
                    </ModalBody>
                    <ModalFooter className='justify-content-between' disabled={upserting}>
                        <Button type='button' color='none' onClick={onClose}>
                            {texts.common.cancel}
                        </Button>

                        <Button type='submit' color='success' disabled={upserting}>
                            <Loader light small visible={upserting} /> {texts.common.save}
                        </Button>
                    </ModalFooter>
                </Form>
            </FormProvider>
        </Modal>
    );
};

export const FormField = ({ property }: { property: IntegrationPropertyDto }) => {
    const label = property.editorLabel || property.name;

    const name = `properties.${property.name}`;

    switch (property.type) {
        case 'Text':
            if (property.allowedValues && property.allowedValues?.length > 0) {
                return (
                    <Forms.Select name={name} options={property.allowedValues.map(value => ({ value, label: value }))}
                        label={label} hints={property.editorDescription} />
                );
            } else {
                if (property.format && property.format !== 'None') {
                    switch (property.format) {
                        case 'Email':
                            return (
                                <Forms.Email name={name}
                                    label={label} hints={property.editorDescription} />
                            );
                        case 'HttpUrl':
                            return (
                                <Forms.Url name={name}
                                    label={label} hints={property.editorDescription} />
                            );
                    }
                }
                return (
                    <Forms.Text name={name}
                        label={label} hints={property.editorDescription} />
                );
            }
        case 'MultilineText':
            return (
                <Forms.Textarea name={name}
                    label={label} hints={property.editorDescription} />
            );
        case 'Number':
            return (
                <Forms.Number name={name}
                    label={label} hints={property.editorDescription} />
            );
        case 'Password':
            return (
                <Forms.Password name={name}
                    label={label} hints={property.editorDescription} />
            );
        case 'Boolean':
            return (
                <Forms.Boolean name={name} asString
                    label={label} hints={property.editorDescription} />
            );
        default:
            return null;
    }
};
