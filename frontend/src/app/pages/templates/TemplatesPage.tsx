/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Icon, Loader } from '@app/framework';
import { TemplateDto } from '@app/service';
import { deleteTemplateAsync, getApp, loadTemplatesAsync, selectTemplate, togglePublishDialog, useApps, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Col, Row, Table } from 'reactstrap';
import { TemplateForm } from './TemplateForm';
import { TemplateRow } from './TemplateRow';

export const TemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const templates = useTemplates(x => x.templates);
    const templateCode = useTemplates(x => x.currentTemplateCode);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);

    const template = React.useMemo(() => {
        return templates.items?.find(x => x.code === templateCode);
    }, [templates, templateCode]);

    React.useEffect(() => {
        dispatch(loadTemplatesAsync(appId));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadTemplatesAsync(appId));
    }, [appId]);

    const doNew = React.useCallback(() => {
        dispatch(selectTemplate({ code: undefined }));
    }, []);

    const doSelect = React.useCallback((template: TemplateDto) => {
        dispatch(selectTemplate({ code: template.code }));
    }, []);

    const doDelete = React.useCallback((template: TemplateDto) => {
        dispatch(deleteTemplateAsync({ appId, code: template.code }));
    }, [appId]);

    const doPublish = React.useCallback((template: TemplateDto) => {
        dispatch(togglePublishDialog({ open: true, values: { templateCode: template.code } }));
    }, []);

    return (
        <div className='templates'>
            <div className='templates-column templates-overview'>
                <Row className='align-items-center templates-header'>
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
                                                onDelete={doDelete} />
                                        ))}
                                    </>
                                }

                                {!templates.isLoading && templates.items && templates.items.length === 0 &&
                                    <tr className='list-item-empty'>
                                        <td colSpan={2}>{texts.templates.templatesNotFound}</td>
                                    </tr>
                                }
                            </>
                        </tbody>
                    </Table>
                </div>
            </div>

            <TemplateForm language={language} onLanguageSelect={setLanguage} template={template} />
        </div>
    );
};
