/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListSearch, Loader, Query, useDialog, useSavedState } from '@app/framework';
import { UserDto } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { deleteUserAsync, loadUsersAsync, openPublishDialog, useApps, useUsers } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { useRouteMatch } from 'react-router';
import ReactTooltip from 'react-tooltip';
import { Button, Col, Row, Table } from 'reactstrap';
import { UserDialog } from './UserDialog';
import { UserRow } from './UserRow';

export const UsersPage = () => {
    const dispatch = useDispatch();
    const match = useRouteMatch();
    const appId = useApps(x => x.appId);
    const dialogEdit = useDialog();
    const dialogNew = useDialog();
    const users = useUsers(x => x.users);
    const [currentUser, setCurrentUser] = React.useState<UserDto>();
    const [hideCounters, setHideCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadUsersAsync(appId, {}));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadUsersAsync(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Query) => {
        dispatch(loadUsersAsync(appId, q));
    }, [appId]);

    const doDelete = React.useCallback((user: UserDto) => {
        dispatch(deleteUserAsync(appId, user.id));
    }, [appId]);

    const doPublish = React.useCallback((user: UserDto) => {
        dispatch(openPublishDialog({ topic: `users/${user.id}` }));
    }, []);

    const doEdit = React.useCallback((user: UserDto) => {
        setCurrentUser(user);

        dialogEdit.open();
    }, []);

    return (
        <div className='users'>
            <Row className='align-items-center header'>
                <Col xs='auto'>
                    <h2>{texts.users.header}</h2>
                </Col>
                <Col>
                    {users.isLoading ? (
                        <Loader visible={users.isLoading} />
                    ) : (
                        <Button color='blank' size='sm' onClick={doRefresh}  data-tip={texts.common.refresh}>
                            <Icon className='text-lg' type='refresh' />
                        </Button>
                    )}
                </Col>
                <Col>
                    <ListSearch list={users} onSearch={doLoad} placeholder={texts.users.searchPlaceholder} />
                </Col>
                <Col xs='auto'>
                    <Button color='success' onClick={dialogNew.open}>
                        <Icon type='add' /> {texts.users.createButton}
                    </Button>
                </Col>
            </Row>

            <FormError error={users.error} />

            <Table className='table-fixed table-simple table-middle'>
                <colgroup>
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
                            <span className='truncate'>{texts.common.actions}</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {users.items &&
                        <>
                            {users.items.map(user => (
                                <UserRow key={user.id} user={user} match={match} hideCounters={hideCounters}
                                    onPublish={doPublish}
                                    onDelete={doDelete}
                                    onEdit={doEdit}
                                />
                            ))}
                        </>
                    }

                    {!users.isLoading && users.items && users.items.length === 0 &&
                        <tr className='list-item-empty'>
                            <td colSpan={4}>{texts.users.usersNotFound}</td>
                        </tr>
                    }
                </tbody>
            </Table>

            <TableFooter hideCounters={hideCounters} list={users}
                onHideCounters={setHideCounters}
                onChange={doLoad} />

            {dialogNew.isOpen &&
                <UserDialog onClose={dialogNew.close} />
            }

            {dialogEdit.isOpen &&
                <UserDialog user={currentUser} onClose={dialogEdit.close} />
            }
        </div>
    );
};
