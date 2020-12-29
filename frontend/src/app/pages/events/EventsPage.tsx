/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListSearch, Loader, Query, useSavedState } from '@app/framework';
import { TableFooter } from '@app/shared/components';
import { loadEventsAsync, useApps, useEvents } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Col, Row, Table } from 'reactstrap';
import { EventRow } from './EventRow';

export const EventsPage = () => {
    const dispatch = useDispatch();
    const appId = useApps(x => x.appId);
    const events = useEvents(x => x.events);
    const [hideCounters, setHideCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadEventsAsync(appId, undefined));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadEventsAsync(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Query) => {
        dispatch(loadEventsAsync(appId, q));
    }, [appId]);

    return (
        <div className='events'>
            <Row className='align-items-center header'>
                <Col xs='auto'>
                    <h2>{texts.events.header}</h2>
                </Col>
                <Col>
                    {events.isLoading ? (
                        <Loader visible={events.isLoading} />
                    ) : (
                        <Button color='blank' size='sm' onClick={doRefresh} data-tip={texts.common.refresh}>
                            <Icon className='text-lg' type='refresh' />
                        </Button>
                    )}
                </Col>
                <Col xs={4}>
                    <ListSearch list={events} onSearch={doLoad} placeholder={texts.events.searchPlaceholder} />
                </Col>
            </Row>

            <FormError error={events.error} />

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
                    <col />
                    <col />
                    <col style={{ width: 250 }} />
                </colgroup>

                <thead>
                    <tr>
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
                                <EventRow key={event.id} event={event} hideCounters={hideCounters} />
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

            <TableFooter hideCounters={hideCounters} list={events}
                onHideCounters={setHideCounters}
                onChange={doLoad} />
        </div>
    );
};
