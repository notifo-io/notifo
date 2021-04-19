/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ConfiguredIntegrationDto, IntegrationDefinitionDto } from '@app/service';
import { getSummaryProperties } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { Badge, Card, CardBody, Col, Row } from 'reactstrap';
import { StatusLabel } from './StatusLabel';

export interface ConfiguredIntegrationProps {
    // The definition.
    definition: IntegrationDefinitionDto;

    // The id of the integration.
    configuredId: string;

    // The defined integration.
    configured: ConfiguredIntegrationDto;

    // Invoked when adding.
    onEdit: (definition: IntegrationDefinitionDto, configured: ConfiguredIntegrationDto, id: string) => void;
}

export const ConfiguredIntegration = React.memo((props: ConfiguredIntegrationProps) => {
    const {
        configured,
        configuredId,
        definition,
        onEdit,
    } = props;

    const doEdit = React.useCallback(() => {
        onEdit(definition, configured, configuredId);
    }, [definition, configured, configuredId]);

    const properties = React.useMemo(() => {
        return getSummaryProperties(definition, configured);
    }, [definition, configured]);

    const clazz = !configured.enabled ? 'text-muted' : '';

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

                            {configured.status !== 'Verified' &&
                                <StatusLabel status={configured.status} />
                            }

                            {configured.test === true &&
                                <Badge pill>{texts.integrations.modeTest}</Badge>
                            }

                            {configured.test === false &&
                                <Badge pill>{texts.integrations.modeProduction}</Badge>
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
