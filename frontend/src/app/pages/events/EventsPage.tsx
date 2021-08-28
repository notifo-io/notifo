/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListSearch, Loader, Query, useSavedState } from '@app/framework';
import { TableFooter } from '@app/shared/components';
import { loadEvents, useApp, useEvents } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Col, Row, Table } from 'reactstrap';
import { EventRow } from './EventRow';

export const EventsPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const events = useEvents(x => x.events);
    const [showCounters, setshowCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadEvents(appId, undefined));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadEvents(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadEvents(appId, q));
    }, [appId]);

    return (
        <div className='events'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2>{texts.events.header}</h2>
                        </Col>
                        <Col xs='auto'>
                            {events.isLoading ? (
                                <Loader visible={events.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} md={7}>
                    <ListSearch list={events} onSearch={doLoad} placeholder={texts.events.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={events.error} />

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
                    <col style={{ width: 50 }} />
                    <col />
                    <col />
                    <col style={{ width: 250 }} />
                </colgroup>

                <thead>
                    <tr>
                        <th>+</th>
                        <th>
                            <span className='truncate'>{texts.common.subject}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.topic}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.timestamp}</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {events.items &&
                        <>
                            {events.items.map(event => (
                                <EventRow key={event.id} event={event} showCounters={showCounters} />
                            ))}
                        </>
                    }

                    {!events.isLoading && events.items && events.items.length === 0 &&
                        <tr className='list-item-empty'>
                            <td colSpan={4}>{texts.events.eventsNotFound}</td>
                        </tr>
                    }
                </tbody>
            </Table>

            <TableFooter showDetails={showCounters} list={events}
                onShowDetails={setshowCounters}
                onChange={doLoad} />
        </div>
    );
};
