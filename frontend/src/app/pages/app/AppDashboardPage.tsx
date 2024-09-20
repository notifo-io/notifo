/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Card, CardBody, Col, FormGroup, Label, Row } from 'reactstrap';
import { ApiValue, Code, Icon } from '@app/framework';
import { getApiUrl } from '@app/service';
import { CounterCards } from '@app/shared/components';
import { useApp } from '@app/state';
import { texts } from '@app/texts';

export const AppDashboardPage = () => {
    const app = useApp()!;

    const sortedKeys = React.useMemo(() => {
        const keys = Object.entries(app.apiKeys).map(x => ({ key: x[0], role: x[1] }));

        keys.sort((x, y) => x.role.localeCompare(y.role));

        return keys;
    }, [app]);

    const apiKey = sortedKeys.filter(x => x.role === 'WebManager').map(x => x.key)[0];

    return (
        <div className='dashboard'>
            <h2>{texts.app.welcome(app.name)}</h2>

            <CounterCards counters={app.counters} />

            <div className='text-right text-muted' style={{ marginTop: '-1rem' }}>
                <small>{texts.common.statisticsInfo}</small>
            </div>

            <h5 className='card-title'>{texts.app.apiDetails}</h5>

            <Card>
                <CardBody>
                    <FormGroup>
                        <Label>{texts.common.appId}</Label>

                        <ApiValue value={app.id} />
                    </FormGroup>

                    {sortedKeys.map(x => (
                        <FormGroup key={x.key}>
                            <Label>{texts.app.apiKey} {x.role}</Label>

                            <ApiValue value={x.key} />
                        </FormGroup>
                    ))}
                </CardBody>
            </Card>

            <h5 className='card-title mt-4'>{texts.common.webPlugin}</h5>

            <Card>
                <CardBody>
                    <Row className='mb-2 align-items-end'>
                        <Col>
                            <Label>{texts.common.webPluginHint}</Label>
                        </Col>
                        <Col xs='auto'>
                            <a className='btn btn-secondary-link' target='_blank' href={`/demo/?apiKey=${apiKey}`}>
                                <Icon type='code' /> {texts.common.demo}
                            </a>
                        </Col>
                    </Row>

                    <Code mode='html' value={buildSampleCode()} autoHeight />
                </CardBody>
            </Card>
        </div>
    );
};

const buildSampleCode = () => {
    return `
<script src="${getApiUrl()}/notifo-sdk.js"></script>
<script>
   var notifo = window['notifo'] || (window['notifo'] = []);
   // Initialize the plugin.
   notifo.push(['init', {
     userToken: 'YOUR_USER_API_KEY'
   }]);

   // Subscribe to web push.
   notifo.push(['subscribe']);
</script>

<div id="notifo-button"></div>
<script>
    // Show the notifications widget.
    notifo.push(['show-notifications', 'notifo-button', { style: 'notifo' }]);
</script>

<div id="topic-button"></div>
<script>
    // Show the topic widget.
    notifo.push(['show-topic', 'topic-button', 'news/general', { style: 'heart' }]);
</script>`.trim();
};
