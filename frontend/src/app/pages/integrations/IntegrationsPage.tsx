/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader } from '@app/framework';
import { ConfiguredIntegrationDto, IntegrationDefinitionDto } from '@app/service';
import { getApp, loadIntegrationAsync, useApps, useIntegrations } from '@app/state';
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
    defined?: ConfiguredIntegrationDto;
    id?: string;
};

export const IntegrationsPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const configured = useIntegrations(x => x.configured);
    const loading = useIntegrations(x => x.loading);
    const loadingError = useIntegrations(x => x.loadingError);
    const supported = useIntegrations(x => x.supported);
    const [selected, setSelected] = React.useState<SelectedIntegration>();

    React.useEffect(() => {
        dispatch(loadIntegrationAsync({ appId }));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadIntegrationAsync({ appId }));
    }, [appId]);

    const doAdd = React.useCallback((definition: IntegrationDefinitionDto, type: string) => {
        setSelected(({ definition, type }));
    }, []);

    const doEdit = React.useCallback((definition: IntegrationDefinitionDto, defined: ConfiguredIntegrationDto, id: string) => {
        setSelected(({ definition, defined, id, type: defined.type }));
    }, []);

    const doClose = React.useCallback(() => {
        setSelected(null!);
    }, []);

    const integrations =
        Object.keys(configured).map(id => {
            const defined = configured[id];

            return {
                id,
                defined,
                definition: supported[defined.type],
            };
        }).filter(x => !!x.definition);

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
                        <Button color='blank' size='sm' onClick={doRefresh}  data-tip={texts.common.refresh}>
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
                        <ConfiguredIntegration key={integration.id}
                            id={integration.id}
                            definition={integration.definition}
                            defined={integration.defined}
                            onEdit={doEdit} />
                    ))}
                </div>
            }

            <div className='mb-4'>
                <h6>{texts.integrations.supported}</h6>

                {Object.keys(supported).map(id => (
                    <SupportedIntegration key={id} type={id} definition={supported[id]}
                        onAdd={doAdd} />
                ))}
            </div>

            {selected &&
                <IntegrationDialog
                    appId={appId}
                    id={selected.id}
                    definition={selected?.definition}
                    defined={selected.defined}
                    onClose={doClose}
                    type={selected.type}
                />
            }
        </div>

    );
};
