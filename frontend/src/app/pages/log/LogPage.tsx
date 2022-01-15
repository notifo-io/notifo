/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, Col, Row, Table } from 'reactstrap';
import { FormError, Icon, ListSearch, Loader, Query } from '@app/framework';
import { TableFooter } from '@app/shared/components';
import { loadLog, useApp, useLog } from '@app/state';
import { texts } from '@app/texts';
import { LogEntryRow } from './LogEntryRow';

export const LogPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const logEntries = useLog(x => x.entries);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadLog(appId, {}));
    }, [dispatch, appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadLog(appId));
    }, [dispatch, appId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadLog(appId, q));
    }, [dispatch, appId]);

    return (
        <div className='log'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2>{texts.log.header}</h2>
                        </Col>
                        <Col xs='auto'>
                            {logEntries.isLoading ? (
                                <Loader visible={logEntries.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' className='btn-flat' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} md={7}>
                    <ListSearch list={logEntries} onSearch={doLoad} placeholder={texts.log.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={logEntries.error} />

            <Card className='card-table'>
                <CardBody>
                    <Table className='table-fixed table-simple table-middle'>
                        <colgroup>
                            <col />
                            <col style={{ width: 160 }} />
                            <col style={{ width: 200 }} />
                            <col style={{ width: 200 }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>
                                    <span className='truncate'>{texts.common.message}</span>
                                </th>
                                <th>
                                    <span className='truncate'>{texts.common.count}</span>
                                </th>
                                <th>
                                    <span className='truncate'>{texts.common.lastSeen}</span>
                                </th>
                                <th>
                                    <span className='truncate'>{texts.common.firstSeen}</span>
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {logEntries.items &&
                                <>
                                    {logEntries.items.map(entry => (
                                        <LogEntryRow key={entry.message} entry={entry} />
                                    ))}
                                </>
                            }

                            {!logEntries.isLoading && logEntries.items && logEntries.items.length === 0 &&
                                <tr>
                                    <td colSpan={4}>{texts.log.logEntriesNotFound}</td>
                                </tr>
                            }
                        </tbody>
                    </Table>
                </CardBody>
            </Card>

            <TableFooter list={logEntries} noDetailButton
                onChange={doLoad} />
        </div>
    );
};
