/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Badge, Card, CardBody, Col, Row } from 'reactstrap';
import { IntegrationDefinitionDto } from '@app/service';

export interface SupportedIntegrationProps {
    // The type code ofthe integration.
    type: string;

    // The definition.
    definition: IntegrationDefinitionDto;

    // Invoked when adding.
    onAdd: (integration: IntegrationDefinitionDto, type: string) => void;
}

export const SupportedIntegration = React.memo((props: SupportedIntegrationProps) => {
    const { definition, onAdd, type } = props;

    const doAdd = () => {
        onAdd(definition, type);
    };

    return (
        <Card className='integration-card' onClick={doAdd}>
            <CardBody>
                <Row noGutters>
                    <Col className='col-image'>
                        <img src={definition.logoUrl} alt={definition.title} />
                    </Col>

                    <Col>
                        <h4>{definition.title}</h4>

                        <div>
                            {definition.capabilities.map(capability => (
                                <Badge key={capability} color='secondary' className='mr-1' pill>{capability}</Badge>
                            ))}
                        </div>

                        <small>
                            {definition.description}
                        </small>
                    </Col>
                </Row>
            </CardBody>
        </Card>
    );
});
