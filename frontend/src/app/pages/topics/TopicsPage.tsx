/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Col, Nav, NavItem, NavLink, Row, Table } from 'reactstrap';
import { FormError, Icon, ListSearch, Loader, Query, useDialog, useSavedState } from '@app/framework';
import { TopicDto, TopicQueryScope } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { deleteTopic, loadTopics, useApp, useTopics } from '@app/state';
import { texts } from '@app/texts';
import { TopicDialog } from './TopicDialog';
import { TopicRow } from './TopicRow';

export const TopicsPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const dialogEdit = useDialog();
    const dialogNew = useDialog();
    const topics = useTopics(x => x.topics);
    const [currentTopic, setCurrentTopic] = React.useState<TopicDto>();
    const [currentScope, setCurrentScope] = React.useState<TopicQueryScope>('All');
    const [showCounters, setShowCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadTopics(appId, currentScope, {}));
    }, [dispatch, appId, currentScope]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadTopics(appId, currentScope));
    }, [dispatch, appId, currentScope]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadTopics(appId, currentScope, q));
    }, [dispatch, appId, currentScope]);

    const doDelete = React.useCallback((topic: TopicDto) => {
        dispatch(deleteTopic({ appId, path: topic.path, scope: currentScope }));
    }, [dispatch, appId, currentScope]);

    const doEdit = React.useCallback((topic: TopicDto) => {
        setCurrentTopic(topic);

        dialogEdit.open();
    }, [dialogEdit]);

    return (
        <div className='topics'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col className='no-overflow'>
                            <Nav className='nav-tabs2'>
                                <NavItem>
                                    <NavLink active={currentScope === 'All'} onClick={() => setCurrentScope('All')}>
                                        {texts.topics.all}
                                    </NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink active={currentScope === 'Explicit'} onClick={() => setCurrentScope('Explicit')}>
                                        {texts.topics.explicit}
                                    </NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink active={currentScope === 'Implicit'} onClick={() => setCurrentScope('Implicit')}>
                                        {texts.topics.implicit}
                                    </NavLink>
                                </NavItem>
                            </Nav>
                        </Col>
                        <Col xs='auto' className='col-refresh'>
                            {topics.isLoading ? (
                                <Loader visible={topics.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' className='btn-flat' onClick={doRefresh} data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} md={7}>
                    <Row noGutters className='flex-nowrap'>
                        <Col>
                            <ListSearch list={topics} onSearch={doLoad} placeholder={texts.log.searchPlaceholder} />
                        </Col>
                        <Col xs='auto pl-2'>
                            <Button color='success' onClick={dialogNew.open}>
                                <Icon type='add' /> {texts.topics.createButton}
                            </Button>
                        </Col>
                    </Row>
                </Col>
            </Row>

            <FormError error={topics.error} />

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
                    <col />
                    <col />
                    <col style={{ width: 100 }} />
                    <col style={{ width: 100 }} />
                    <col style={{ width: 130 }} />
                </colgroup>

                <thead>
                    <tr>
                        <th>
                            <span className='truncate'>{texts.common.topic}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.name}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.topics.showAutomatically}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.topics.explicit}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.actions}</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {topics.items &&
                        <>
                            {topics.items.map(topic => (
                                <TopicRow key={topic.path} topic={topic} language={appLanguages[0]} showCounters={showCounters} 
                                    onDelete={doDelete}
                                    onEdit={doEdit}
                                />
                            ))}
                        </>
                    }

                    {!topics.isLoading && topics.items && topics.items.length === 0 &&
                        <tr className='list-item-empty'>
                            <td colSpan={5}>{texts.topics.topicsNotFound}</td>
                        </tr>
                    }
                </tbody>
            </Table>

            <TableFooter list={topics} showDetails={showCounters}
                onChange={doLoad}
                onShowDetails={setShowCounters} />

            {dialogNew.isOpen &&
                <TopicDialog scope={currentScope} onClose={dialogNew.close} />
            }

            {dialogEdit.isOpen &&
                <TopicDialog topic={currentTopic} scope={currentScope} onClose={dialogEdit.close} />
            }
        </div>
    );
};
