/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader, useDialog } from '@app/framework';
import { selectApp, useApps } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Link, useRouteMatch } from 'react-router-dom';
import { Button, Card, CardBody, Col, Row } from 'reactstrap';
import { AppDialog } from './AppDialog';

export const AppsPage = () => {
    const dispatch = useDispatch();
    const apps = useApps(x => x.apps);
    const dialogNew = useDialog();
    const match = useRouteMatch();

    React.useEffect(() => {
        dispatch(selectApp({ appId: undefined }));
    }, [dispatch]);

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs='auto'>
                    <h1>{texts.common.apps}</h1>
                </Col>
                <Col>
                    <Loader visible={apps.isLoading} />
                </Col>
                <Col xs='auto'>
                    <Button color='success' onClick={dialogNew.open}>
                        <Icon type='add' /> {texts.apps.createButton}
                    </Button>
                </Col>
            </Row>

            <FormError error={apps.error} />

            {dialogNew.isOpen &&
                <AppDialog onClose={dialogNew.close}></AppDialog>
            }

            {apps.items &&
                <div>
                    {apps.items.map(app => (
                        <Link key={app.id} to={`${match.path}/${app.id}`} className='card-link'>
                            <Card className='app-card'>
                                <CardBody>
                                    <h4 className='truncate'>{app.name || texts.common.unnamed}</h4>
                                </CardBody>
                            </Card>
                        </Link>
                    ))}
                </div>
            }
        </>
    );
};
