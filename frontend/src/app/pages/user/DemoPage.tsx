/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import ReactMarkdown from 'react-markdown';
import { useParams } from 'react-router';
import { useSearchParams } from 'react-router-dom';
import { Card, CardBody, Col, Container, Row } from 'reactstrap';
import { texts } from '@app/texts';

export const DemoPage = () => {
    const userToken = useParams().userId!;
    const [params] = useSearchParams();

    React.useEffect(() => {
        const notifo = (window as any)['notifo'] || ((window as any)['notifo'] = []);

        const args = {
            linkTarget: '_blank',
            apiUrl: '/',
            apiKey: params.get('apiKey'),
            userToken,
            onNotification: (notification: any) => {
                console.log(`Received: ${JSON.stringify(notification, undefined, 2)}`);
            },
            onConfirm: (notification: any) => {
                console.log(`Confirmed: ${JSON.stringify(notification, undefined, 2)}`);
            },
        };

        notifo.push(['init', args]);

        notifo.push(['show-notifications', 'button1', { style: 'notifo', position: 'bottom-left' }]);

        notifo.push(['show-topic', 'topic1', 'updates/lego', { style: 'heart' }]);
        notifo.push(['show-topic', 'topic2', 'updates/tech', { style: 'alarm' }]);
        notifo.push(['show-topic', 'topic3', 'updates/sport',  { style: 'bell' }]);
        notifo.push(['show-topic', 'topic4', 'updates/games', { style: 'star' }]);

        notifo.push(['subscribe']);
    }, [params, userToken]);

    return (
        <Container className='demo-container'>
            <div className='mt-4 mb-3 text-right'>
                <img className='demo-logo' src='/logo.svg' alt='Logo' />
            </div>

            <Card>
                <CardBody>
                    <h3>{texts.demo.title}</h3>
                </CardBody>

                <CardBody>
                    <Row className='align-items-center'>
                        <Col xs='auto'>
                            <span id='button1'></span>
                        </Col>

                        <Col>
                            {texts.demo.featureButton}
                        </Col>
                    </Row>

                    <hr />

                    <Row className='align-items-center'>
                        <Col xs='auto'>
                            <span id='topic1'></span>
                        </Col>

                        <Col>
                            <ReactMarkdown className='demo-text'>{texts.demo.featureSubscribe('updates/lego', 'heart')}</ReactMarkdown>
                        </Col>
                    </Row>

                    <hr />

                    <Row className='align-items-center'>
                        <Col xs='auto'>
                            <span id='topic2'></span>
                        </Col>

                        <Col>
                            <ReactMarkdown className='demo-text'>{texts.demo.featureSubscribe('updates/tech', 'alarm')}</ReactMarkdown>
                        </Col>
                    </Row>

                    <hr />

                    <Row className='align-items-center'>
                        <Col xs='auto'>
                            <span id='topic3'></span>
                        </Col>

                        <Col>
                            <ReactMarkdown className='demo-text'>{texts.demo.featureSubscribe('updates/sport', 'bell')}</ReactMarkdown>
                        </Col>
                    </Row>

                    <hr />

                    <Row className='align-items-center'>
                        <Col xs='auto'>
                            <span id='topic4'></span>
                        </Col>

                        <Col>
                            <ReactMarkdown className='demo-text'>{texts.demo.featureSubscribe('updates/games', 'star')}</ReactMarkdown>
                        </Col>
                    </Row>
                </CardBody>
            </Card>
        </Container>
    );
};