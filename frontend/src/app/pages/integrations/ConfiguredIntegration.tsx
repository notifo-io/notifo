/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ConfiguredIntegrationDto, IntegrationDefinitionDto } from '@app/service';
import * as React from 'react';
import { Badge, Card, CardBody, Col, Row } from 'reactstrap';
import { StatusLabel } from './StatusLabel';

export interface ConfiguredIntegrationProps {
    // The id of the integration.
    id: string;

    // The definition.
    definition: IntegrationDefinitionDto;

    // The defined integration.
    defined: ConfiguredIntegrationDto;

    // Invoked when adding.
    onEdit: (definition: IntegrationDefinitionDto, defined: ConfiguredIntegrationDto, id: string) => void;
}

export const ConfiguredIntegration = React.memo((props: ConfiguredIntegrationProps) => {
    const {
        definition,
        defined,
        id,
        onEdit,
    } = props;

    const doEdit = React.useCallback(() => {
        onEdit(definition, defined, id);
    }, [definition, defined, id]);

    const properties =
        definition.properties.filter(p => !!p.summary)
            .map(p => ({ label: p.editorLabel || p.name, value: defined.properties[p.name] }));

    const clazz = !defined.enabled ? 'text-muted' : '';

    return (
        <Card className='integration-card' onClick={doEdit}>
            <CardBody>
                <Row noGutters>
                    <Col className='col-image'>
                        <img src={definition.logoUrl} alt={definition.title} />
                    </Col>

                    <Col className='no-overflow'>
                        <h4 className={clazz}>{definition.title}</h4>

                        <div>
                            {definition.capabilities.map(capability => (
                                <Badge key={capability} className='mr-1' color='secondary' pill>{capability}</Badge>
                            ))}

                            {defined.status !== 'Verified' &&
                                <StatusLabel status={defined.status} />
                            }
                        </div>

                        <div>
                            {properties.map(property => (
                                <div key={property.label} className='mt-2'>
                                    <small>
                                        <strong>{property.label}</strong>: {property.value}
                                    </small>
                                </div>
                            ))}
                        </div>
                    </Col>
                </Row>
            </CardBody>
        </Card>
    );
});
