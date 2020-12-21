/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, Loader } from '@app/framework';
import { TemplateDto } from '@app/service';
import { deleteTemplateAsync, loadTemplatesAsync, openPublishDialog, selectTemplate, useApps, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, CardHeader, Col, Row, Table } from 'reactstrap';
import { TemplateForm } from './TemplateForm';
import { TemplateRow } from './TemplateRow';

export const TemplatesPage = () => {
    const dispatch = useDispatch();
    const appId = useApps(x => x.appId);
    const templates = useTemplates(x => x.templates);
    const templateCode = useTemplates(x => x.currentTemplateCode);

    const template = React.useMemo(() => {
        return templates.items?.find(x => x.code === templateCode);
    }, [templates, templateCode]);

    React.useEffect(() => {
        dispatch(loadTemplatesAsync(appId, true));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadTemplatesAsync(appId));
    }, [appId]);

    const doNew = React.useCallback(() => {
        dispatch(selectTemplate(undefined));
    }, []);

    const doSelect = React.useCallback((template: TemplateDto) => {
        dispatch(selectTemplate(template.code));
    }, []);

    const doDelete = React.useCallback((template: TemplateDto) => {
        dispatch(deleteTemplateAsync(appId, template.code));
    }, [appId]);

    const doPublish = React.useCallback((template: TemplateDto) => {
        dispatch(openPublishDialog({ templateCode: template.code }));
    }, []);

    return (
        <div className='templates'>
            <Row>
                <Col className='overview'>
                    <div className='overview-inner'>
                        <Row className='align-items-center header'>
                            <Col xs='auto'>
                                <h2 className='truncate'>{texts.templates.header}</h2>
                            </Col>
                            <Col>
                                {templates.isLoading ? (
                                    <Loader visible={templates.isLoading} />
                                ) : (
                                    <Button color='blank' size='sm' onClick={doRefresh}  data-tip={texts.common.refresh}>
                                        <Icon className='text-lg' type='refresh' />
                                    </Button>
                                )}
                            </Col>
                            <Col xs='auto'>
                                <Button color='success' onClick={doNew}>
                                    <Icon type='add' /> {texts.templates.createButton}
                                </Button>
                            </Col>
                        </Row>

                        <FormError error={templates.error} />

                        <Card>
                            <CardHeader>
                                <Table className='table-fixed table-simple table-middle'>
                                    <colgroup>
                                        <col />
                                        <col style={{ width: 170 }} />
                                    </colgroup>

                                    <thead>
                                        <tr>
                                            <th>
                                                <span className='truncate'>{texts.common.code}</span>
                                            </th>
                                            <th className='text-right'>
                                                <span className='truncate'>{texts.common.actions}</span>
                                            </th>
                                        </tr>
                                    </thead>
                                </Table>
                            </CardHeader>
                            <CardBody>
                                <Table className='table-fixed table-simple table-middle'>
                                    <colgroup>
                                        <col />
                                        <col style={{ width: 170 }} />
                                    </colgroup>

                                    <tbody>
                                        {templates.items &&
                                            <>
                                                {templates.items.map(template => (
                                                    <TemplateRow key={template.code} template={template}
                                                        onPublish={doPublish}
                                                        onEdit={doSelect}
                                                        onDelete={doDelete} />
                                                ))}
                                            </>
                                        }

                                        {!templates.isLoading && templates.items && templates.items.length === 0 &&
                                            <tr>
                                                <td colSpan={2}>{texts.templates.templatesNotFound}</td>
                                            </tr>
                                        }
                                    </tbody>
                                </Table>
                            </CardBody>
                        </Card>
                    </div>
                </Col>

                <Col style={{ overflow: 'hidden' }}>
                    {template ? (
                        <h2 className='truncate'>{texts.templates.templateEdit} {template.code}</h2>
                    ) : (
                        <h2 className='truncate'>{texts.templates.templateNew}</h2>
                    )}

                    <Card>
                        <CardBody>
                            <TemplateForm template={template} />
                        </CardBody>
                    </Card>
                </Col>
            </Row>
        </div>
    );
};
