/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Col, Row, Table } from 'reactstrap';
import { FormError, Icon, ListSearch, Loader, Query, useEventCallback, useSavedState } from '@app/framework';
import { TableFooter } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { loadEvents, useApp, useEvents } from '@app/state';
import { texts } from '@app/texts';
import { EventRow } from './EventRow';

const NON_WEBHOOKS = CHANNELS.filter(x => x !== 'webhook');

export const EventsPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const events = useEvents(x => x.events);
    const [showCounters, setshowCounters] = useSavedState(false, 'show.counters');
    const [channels, setChannels] = React.useState<string[]>([]);

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadEvents(appId, {}, undefined, channels));
    }, [dispatch, appId, channels]);

    const doRefresh = useEventCallback(() => {
        dispatch(loadEvents(appId, undefined, undefined, channels));
    });

    const doLoad = useEventCallback((q?: Partial<Query>) => {
        dispatch(loadEvents(appId, q, undefined, channels));
    });

    const doToggleChannel = useEventCallback((channel: string) => {
        setChannels(channels => {
            let newChannels: string[];

            if (channels.indexOf(channel) >= 0) {
                newChannels = channels.filter(x => x !== channel);
            } else {
                newChannels = [...channels, channel];
            }

            return newChannels.length >= NON_WEBHOOKS.length ? [] : newChannels;
        });
    });

    return (
        <div className='events'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2>{texts.events.header}</h2>
                        </Col>
                        <Col xs='auto' className='col-refresh'>
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

            <Row className='channels-filter' noGutters>
                {NON_WEBHOOKS.map(channel => 
                    <Col key={channel} style={{ width: `100/${NON_WEBHOOKS.length}%` }}>
                        <Button block color='blank' className={classNames('btn-flat', { active: channels.indexOf(channel) >= 0 })} onClick={() => doToggleChannel(channel)}>
                            {texts.notificationSettings[channel].name}
                        </Button>
                    </Col>,
                )}
            </Row>

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
                    <col style={{ width: 50 }} />
                    <col />
                    <col />
                    <col style={{ width: 200 }} />
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
                        <th className='text-right'>
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

                    {events.isLoaded && events.items && events.items.length === 0 &&
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
