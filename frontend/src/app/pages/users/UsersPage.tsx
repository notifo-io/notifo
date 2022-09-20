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
import { Button, Col, Row, Table } from 'reactstrap';
import { FormError, Icon, ListSearch, Loader, Query, useBooleanObj, useEventCallback, useSavedState } from '@app/framework';
import { UserDto } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { deleteUser, loadUsers, togglePublishDialog, useApp, useUsers } from '@app/state';
import { texts } from '@app/texts';
import { UserDialog } from './UserDialog';
import { UserRow } from './UserRow';

export const UsersPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const dialogEdit = useBooleanObj();
    const dialogNew = useBooleanObj();
    const match = useRouteMatch();
    const users = useUsers(x => x.users);
    const [currentUser, setCurrentUser] = React.useState<UserDto>();
    const [showCounters, setShowCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadUsers(appId, {}));
    }, [dispatch, appId]);

    const doRefresh = useEventCallback(() => {
        dispatch(loadUsers(appId));
    });

    const doLoad = useEventCallback((q?: Partial<Query>) => {
        dispatch(loadUsers(appId, q));
    });

    const doDelete = useEventCallback((user: UserDto) => {
        dispatch(deleteUser({ appId, userId: user.id }));
    });

    const doPublish = useEventCallback((user: UserDto) => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${user.id}` } }));
    });

    const doEdit = useEventCallback((user: UserDto) => {
        setCurrentUser(user);

        dialogEdit.on();
    });

    return (
        <div className='users'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2 className='truncate'>{texts.users.header}</h2>
                        </Col>
                        <Col xs='auto' className='col-refresh'>
                            {users.isLoading ? (
                                <Loader visible={users.isLoading} />
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
                            <ListSearch list={users} onSearch={doLoad} placeholder={texts.users.searchPlaceholder} />
                        </Col>
                        <Col xs='auto pl-2'>
                            <Button color='success' onClick={dialogNew.on}>
                                <Icon type='add' /> {texts.users.createButton}
                            </Button>
                        </Col>
                    </Row>
                </Col>
            </Row>

            <FormError error={users.error} />

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
                    <col />
                    <col />
                    <col />
                    <col />
                    <col />
                    <col style={{ width: 170 }} />
                </colgroup>

                <thead>
                    <tr>
                        <th>
                            <span className='truncate'>{texts.common.id}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.name}</span>
                        </th>
                        <th>
                            <span className='truncate'>{texts.common.email}</span>
                        </th>
                        <th className='text-right'>
                            <span className='truncate'>{texts.common.lastNotification}</span>
                        </th>
                        <th className='text-right'>
                            <span className='truncate'>{texts.common.lastUpdate}</span>
                        </th>
                        <th className='text-right'>
                            <span className='truncate'>{texts.common.actions}</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {users.items &&
                        <>
                            {users.items.map(user => (
                                <UserRow key={user.id} user={user} match={match} showCounters={showCounters}
                                    onPublish={doPublish}
                                    onDelete={doDelete}
                                    onEdit={doEdit}
                                />
                            ))}
                        </>
                    }

                    {users.isLoaded && users.items && users.items.length === 0 &&
                        <tr className='list-item-empty'>
                            <td colSpan={6}>{texts.users.usersNotFound}</td>
                        </tr>
                    }
                </tbody>
            </Table>

            <TableFooter showDetails={showCounters} list={users}
                onShowDetails={setShowCounters}
                onChange={doLoad} />

            {dialogNew.value &&
                <UserDialog onClose={dialogNew.off} />
            }

            {dialogEdit.value &&
                <UserDialog user={currentUser} onClose={dialogEdit.off} />
            }
        </div>
    );
};
