/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Icon, ListPager, ListSearch, Loader, Query, useDialog } from '@app/framework';
import { SubscriptionDto } from '@app/service';
import { deleteSubscriptionAsync, loadSubscriptionsAsync, togglePublishDialog, useApps, useSubscriptions } from '@app/state';
import { texts } from '@app/texts';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import ReactTooltip from 'react-tooltip';
import { Button, Card, CardBody, Col, Row, Table } from 'reactstrap';
import { SubscriptionDialog } from './SubscriptionDialog';
import { SubscriptionRow } from './SubscriptionRow';

export interface SubscriptionsProps {
    // The user id.
    userId: string;
}

export const Subscriptions = (props: SubscriptionsProps) => {
    const { userId } = props;

    const dispatch = useDispatch();
    const appId = useApps(x => x.appId);
    const dialogEdit = useDialog();
    const dialogNew = useDialog();
    const subscriptions = useSubscriptions(x => x.subscriptions);
    const [editSubscription, setEditSubscription] = React.useState<SubscriptionDto>();

    React.useEffect(() => {
        ReactTooltip.rebuild();
    });

    React.useEffect(() => {
        dispatch(loadSubscriptionsAsync(appId, userId, {}));
    }, [appId, userId]);

    const doRefresh = React.useCallback(() => {
        dispatch(loadSubscriptionsAsync(appId, userId));
    }, [appId, userId]);

    const doLoad = React.useCallback((q?: Query) => {
        dispatch(loadSubscriptionsAsync(appId, userId, q));
    }, [appId, userId]);

    const doDelete = React.useCallback((subscription: SubscriptionDto) => {
        dispatch(deleteSubscriptionAsync({ appId, userId, prefix: subscription.topicPrefix }));
    }, [appId, userId]);

    const doPublish = React.useCallback((subscription: SubscriptionDto) => {
        dispatch(togglePublishDialog({ open: true, values: { topic: subscription.topicPrefix } }));
    }, []);

    const doEdit = React.useCallback((subscription: SubscriptionDto) => {
        dialogEdit.open();

        setEditSubscription(subscription);
    }, [dialogEdit.open]);

    return (
        <>
            <Row className='align-items-center header'>
                <Col xs={12} lg={5}>
                    <Row className='align-items-center flex-nowrap'>
                        <Col>
                            <h2 className='truncate'>{texts.subscriptions.header}</h2>
                        </Col>
                        <Col xs='auto'>
                            {subscriptions.isLoading ? (
                                <Loader visible={subscriptions.isLoading} />
                            ) : (
                                <Button color='blank' size='sm' onClick={doRefresh}  data-tip={texts.common.refresh}>
                                    <Icon className='text-lg' type='refresh' />
                                </Button>
                            )}
                        </Col>
                    </Row>
                </Col>
                <Col xs={12} lg={7}>
                    <Row noGutters>
                        <Col>
                            <ListSearch list={subscriptions} onSearch={doLoad} placeholder={texts.subscriptions.searchPlaceholder} />
                        </Col>
                        <Col xs='auto pl-2'>
                            <Button color='success' onClick={dialogNew.open}>
                                <Icon type='add' /> {texts.subscriptions.createButton}
                            </Button>
                        </Col>
                    </Row>
                </Col>
            </Row>

            <FormError error={subscriptions.error} />

            {dialogNew.isOpen &&
                <SubscriptionDialog userId={userId} onClose={dialogNew.close} />
            }

            <Card>
                <CardBody>
                    <Table className='table-fixed table-simple table-middle'>
                        <colgroup>
                            <col />
                            <col style={{ width: 170 }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>
                                    <span className='truncate'>{texts.common.topic}</span>
                                </th>
                                <th className='text-right'>
                                    <span className='truncate'>{texts.common.actions}</span>
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {subscriptions.items &&
                                <>
                                    {subscriptions.items.map(subscription => (
                                        <SubscriptionRow key={subscription.topicPrefix} subscription={subscription}
                                            onPublish={doPublish}
                                            onDelete={doDelete}
                                            onEdit={doEdit}
                                        />
                                    ))}
                                </>
                            }

                            {!subscriptions.isLoading && subscriptions.items && subscriptions.items.length === 0 &&
                                <tr>
                                    <td colSpan={4}>{texts.subscriptions.subscriptionsNotFound}</td>
                                </tr>
                            }
                        </tbody>
                    </Table>

                    <ListPager list={subscriptions} onChange={doLoad} />
                </CardBody>
            </Card>

            {dialogEdit.isOpen &&
                <SubscriptionDialog userId={userId} subscription={editSubscription} onClose={dialogEdit.close} />
            }
        </>
    );
};
