/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListSearch, Loader, Query, useDialog, useSavedState } from '@app/framework';
import { UserDto } from '@app/service';
import { TableFooter } from '@app/shared/components';
import { deleteUserAsync, getApp, loadUsersAsync, togglePublishDialog, useApps, useUsers } from '@app/state';
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
    const app = useApps(getApp);
    const appId = app.id;
    const dialogEdit = useDialog();
    const dialogNew = useDialog();
    const users = useUsers(x => x.users);
    const [currentUser, setCurrentUser] = React.useState<UserDto>();
    const [showCounters, setShowCounters] = useSavedState(false, 'show.counters');

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadUsersAsync(appId, {}));
    }, [appId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadUsersAsync(appId));
    }, [appId]);

    const doLoad = React.useCallback((q?: Partial<Query>) => {
        dispatch(loadUsersAsync(appId, q));
    }, [appId]);

    const doDelete = React.useCallback((user: UserDto) => {
        dispatch(deleteUserAsync({ appId, userId: user.id }));
    }, [appId]);

    const doPublish = React.useCallback((user: UserDto) => {
        dispatch(togglePublishDialog({ open: true, values: { topic: `users/${user.id}` } }));
    }, []);

    const doEdit = React.useCallback((user: UserDto) => {
        setCurrentUser(user);

        dialogEdit.open();
    }, []);

    return (
        <div className='users'>
            <Row className='align-items-center header'>
                <Col xs={12} md={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2 className='truncate'>{texts.users.header}</h2>
                        </Col>
                        <Col xs='auto'>
                            {users.isLoading ? (
                                <Loader visible={users.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' onClick={doRefresh}  data-tip={texts.common.refresh}>
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
                            <Button color='success' onClick={dialogNew.open}>
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
                                <UserRow key={user.id} user={user} match={match} showCounters={showCounters}
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

            <TableFooter showDetails={showCounters} list={users}
                onShowDetails={setShowCounters}
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
