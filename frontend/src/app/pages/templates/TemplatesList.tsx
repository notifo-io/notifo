/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Row, Table } from 'reactstrap';
import { Icon, Loader } from '@app/framework';
import { TemplateDto } from '@app/service';
import { deleteTemplate, loadTemplates, selectTemplate, togglePublishDialog, useApp, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import { TemplateRow } from './TemplateRow';

export interface TemplateListProps {
    // Triggered when opened.
    onOpen: () => void;
}

export const TemplatesList = (props: TemplateListProps) => {
    const { onOpen } = props;

    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const selectedTemplateCode = useTemplates(x => x.currentTemplateCode);
    const templates = useTemplates(x => x.templates);

    React.useEffect(() => {
        dispatch(loadTemplates(appId));
    }, [dispatch, appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadTemplates(appId));
    }, [dispatch, appId]);

    const doNew = React.useCallback(() => {
        onOpen();

        dispatch(selectTemplate({ code: undefined }));
    }, [dispatch, onOpen]);

    const doSelect = React.useCallback((template: TemplateDto) => {
        onOpen();

        dispatch(selectTemplate({ code: template.code }));
    }, [dispatch, onOpen]);

    const doDelete = React.useCallback((template: TemplateDto) => {
        dispatch(deleteTemplate({ appId, code: template.code }));
    }, [dispatch, appId]);

    const doPublish = React.useCallback((template: TemplateDto) => {
        dispatch(togglePublishDialog({ open: true, values: { templateCode: template.code } }));
    }, [dispatch]);

    return (
        <div className='templates-column templates-overview'>
            <Row className='align-items-center templates-header'>
                <Col xs='auto'>
                    <h2 className='truncate'>{texts.templates.header}</h2>
                </Col>
                <Col>
                    {templates.isLoading ? (
                        <Loader visible={templates.isLoading} />
                    ) : (
                        <Button color='blank' size='sm' className='btn-flat' onClick={doRefresh} data-tip={texts.common.refresh}>
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

            <div className='templates-body'>
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
                    <tbody>
                        <>
                            {templates.items &&
                                <>
                                    {templates.items.map(template => (
                                        <TemplateRow key={template.code} template={template}
                                            onPublish={doPublish}
                                            onEdit={doSelect}
                                            onDelete={doDelete}
                                            selected={template.code === selectedTemplateCode} />
                                    ))}
                                </>
                            }

                            {templates.isLoaded && templates.items && templates.items.length === 0 &&
                                <tr className='list-item-empty'>
                                    <td colSpan={2}>{texts.templates.templatesNotFound}</td>
                                </tr>
                            }
                        </>
                    </tbody>
                </Table>
            </div>
        </div>
    );
};
