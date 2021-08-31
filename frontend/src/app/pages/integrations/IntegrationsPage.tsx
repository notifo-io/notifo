/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader } from '@app/framework';
import { ConfiguredIntegrationDto, IntegrationDefinitionDto } from '@app/service';
import { getSortedIntegrations, loadIntegrations, useApp, useIntegrations } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Row } from 'reactstrap';
import { ConfiguredIntegration } from './ConfiguredIntegration';
import { IntegrationDialog } from './IntegrationDialog';
import { SupportedIntegration } from './SupportedIntegration';

type SelectedIntegration = {
    type: string;
    definition: IntegrationDefinitionDto;
    configured?: ConfiguredIntegrationDto;
    configuredId?: string;
};

const DEFAULTS: any = {};

export const IntegrationsPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const configured = useIntegrations(x => x.configured || DEFAULTS);
    const definitions = useIntegrations(x => x.supported || DEFAULTS);
    const loading = useIntegrations(x => x.loading);
    const loadingError = useIntegrations(x => x.loadingError);
    const [selected, setSelected] = React.useState<SelectedIntegration>();

    React.useEffect(() => {
        dispatch(loadIntegrations({ appId }));
    }, [dispatch, appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadIntegrations({ appId }));
    }, [dispatch, appId]);

    const doAdd = React.useCallback((definition: IntegrationDefinitionDto, type: string) => {
        setSelected(({ definition, type }));
    }, []);

    const doEdit = React.useCallback((definition: IntegrationDefinitionDto, defined: ConfiguredIntegrationDto, id: string) => {
        setSelected(({ definition, configured: defined, configuredId: id, type: defined.type }));
    }, []);

    const doClose = React.useCallback(() => {
        setSelected(null!);
    }, []);

    const integrations = React.useMemo(() => {
        return getSortedIntegrations(definitions, configured);
    }, [definitions, configured]);

    return (
        <div className='integrations'>
            <Row className='align-items-center header'>
                <Col>
                    <h2>{texts.integrations.header}</h2>
                </Col>
                <Col xs='auto'>
                    {loading ? (
                        <Loader visible={loading} />
                    ) : (
                        <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                            <Icon className='text-lg' type='refresh' />
                        </Button>
                    )}
                </Col>
            </Row>

            <FormError error={loadingError} />

            {integrations.length > 0 &&
                <div className='mb-4'>
                    <h6>{texts.integrations.configured}</h6>

                    {integrations.map(integration => (
                        <ConfiguredIntegration key={integration.configuredId}
                            configured={integration.configured}
                            configuredId={integration.configuredId}
                            definition={integration.definition}
                            onEdit={doEdit}
                        />
                    ))}
                </div>
            }

            <div className='mb-4'>
                <h6>{texts.integrations.supported}</h6>

                {Object.keys(definitions).map(id => (
                    <SupportedIntegration key={id} type={id} definition={definitions[id]}
                        onAdd={doAdd} />
                ))}
            </div>

            {selected &&
                <IntegrationDialog
                    appId={appId}
                    configured={selected.configured}
                    configuredId={selected.configuredId}
                    definition={selected?.definition}
                    onClose={doClose}
                    type={selected.type}
                />
            }
        </div>

    );
};
