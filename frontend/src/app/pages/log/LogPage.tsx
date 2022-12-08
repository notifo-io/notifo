/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, Col, Row, Table } from 'reactstrap';
import { FormError, Icon, ListMultiFilter, ListSearch, Loader, Query, useEventCallback } from '@app/framework';
import { TableFooter } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { loadLog, useApp, useLog } from '@app/state';
import { texts } from '@app/texts';
import { LogEntryRow } from './LogEntryRow';

const SYSTEMS = [...CHANNELS, 'System'].map(value => ({
    value,
    label: texts.notificationSettings[value]?.name || value,
}));

export const LogPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const app = useApp()!;
    const appId = app.id;
    const logEntries = useLog(x => x.entries);
    const userId = match.params['userId'];
    const [systems, setSystems] = React.useState<string[]>([]);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadLog(appId, {}, false, systems, userId));
    }, [dispatch, appId, systems, userId]);

    const doRefresh = useEventCallback(() => {
        dispatch(loadLog(appId, undefined, false, systems, userId));
    });

    const doLoad = useEventCallback((q?: Partial<Query>) => {
        dispatch(loadLog(appId, q, false, systems, userId));
    });

    return (
        <div className='log'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            {userId ? (
                                <h2>{texts.log.userHeader} '{userId}'</h2>
                            ) : (
                                <h2>{texts.log.header}</h2>
                            )}
                        </Col>
                        <Col xs='auto' className='col-refresh'>
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

            <ListMultiFilter value={systems} onChange={setSystems} options={SYSTEMS} />

            <Card className='card-table'>
                <CardBody>
                    <div>
                        <Table className='table-fixed table-simple table-middle'>
                            <colgroup>
                                <col style={{ width: 80 }} />
                                <col style={{ width: 160 }} />
                                <col />
                                <col style={{ width: 80 }} />
                                <col style={{ width: 200 }} />
                                <col style={{ width: 200 }} />
                            </colgroup>

                            <thead>
                                <tr>
                                    <th>
                                        <span className='truncate'>{texts.common.code}</span>
                                    </th>
                                    <th>
                                        <span className='truncate'>{texts.common.system}</span>
                                    </th>
                                    <th>
                                        <span className='truncate'>{texts.common.message}</span>
                                    </th>
                                    <th>
                                        <span className='truncate'>{texts.common.count}</span>
                                    </th>
                                    <th className='text-right'>
                                        <span className='truncate'>{texts.common.lastSeen}</span>
                                    </th>
                                    <th className='text-right'>
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

                                {logEntries.isLoaded && logEntries.items && logEntries.items.length === 0 &&
                                    <tr>
                                        <td colSpan={6}>{texts.log.logEntriesNotFound}</td>
                                    </tr>
                                }
                            </tbody>
                        </Table>
                    </div>
                </CardBody>
            </Card>

            <TableFooter list={logEntries} noDetailButton
                onChange={doLoad} />
        </div>
    );
};
