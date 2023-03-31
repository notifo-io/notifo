/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { Badge, Card, CardBody, Col, Row } from 'reactstrap';
import { useEventCallback } from '@app/framework';
import { ConfiguredIntegrationDto, IntegrationDefinitionDto } from '@app/service';
import { getSummaryProperties } from '@app/state';
import { texts } from '@app/texts';
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

    const doEdit = useEventCallback(() => {
        onEdit(definition, configured, configuredId);
    });

    const properties = React.useMemo(() => {
        return getSummaryProperties(definition, configured);
    }, [definition, configured]);

    return (
        <Card className='integration-card' onClick={doEdit}>
            <CardBody>
                <Row noGutters>
                    <Col className='col-image'>
                        <div dangerouslySetInnerHTML={{ __html: definition.logo }}></div>
                    </Col>

                    <Col className='no-overflow'>
                        <h4 className={classNames({ 'text-muted': !configured.enabled })}>{definition.title}</h4>

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

                            {configured.enabled &&
                                <Badge color='danger' pill>{texts.common.disabled}</Badge>
                            }

                            {!!configured.condition &&
                                <Badge pill>{texts.integrations.condition}</Badge>
                            }

                            {!!configured.priority &&
                                <Badge pill>{texts.common.prio} {configured.priority}</Badge>
                            }
                        </div>

                        <div>
                            {properties.filter(x => !!x.value).map(property => (
                                <div key={property.label} className='mt-2'>
                                    <small>
                                        <strong>{property.label}</strong><br />{property.value}
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
