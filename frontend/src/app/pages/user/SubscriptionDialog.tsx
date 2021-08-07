/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader } from '@app/framework';
import { SubscriptionDto } from '@app/service';
import { NotificationsForm } from '@app/shared/components';
import { getApp, upsertSubscription, useApps, useSubscriptions } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as Yup from 'yup';

const FormSchema = Yup.object().shape({
    // Required topic name.
    topicPrefix: Yup.string()
        .label(texts.common.topic).requiredI18n().topicI18n(),
});

export interface SubscriptionDialogProps {
    // The subscription to edit.
    subscription?: SubscriptionDto;

    // The user id.
    userId: string;

    // Invoked when closed.
    onClose?: () => void;
}

export const SubscriptionDialog = (props: SubscriptionDialogProps) => {
    const { onClose, subscription, userId } = props;

    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const upserting = useSubscriptions(x => x.upserting);
    const upsertingError = useSubscriptions(x => x.upsertingError);
    const [wasUpserting, setWasUpserting] = React.useState(false);

    React.useEffect(() => {
        if (upserting) {
            setWasUpserting(true);
        }
    }, [upserting]);

    React.useEffect(() => {
        if (!upserting && wasUpserting && !upsertingError) {
            doCloseForm();
        }
    }, [upserting, upsertingError, wasUpserting]);

    const doCloseForm = React.useCallback(() => {
        if (onClose) {
            onClose();
        }
    }, []);

    const doSave = React.useCallback((params: SubscriptionDto) => {
        dispatch(upsertSubscription({ appId, userId, params }));
    }, [appId, userId]);

    const initialValues = subscription || { topicPrefix: '' };

    return (
        <Modal isOpen={true} size='lg' toggle={doCloseForm}>
            <Formik<SubscriptionDto> initialValues={initialValues} onSubmit={doSave} enableReinitialize validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {subscription ? texts.subscriptions.editHeader : texts.subscriptions.createHeader}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset disabled={upserting}>
                                <Forms.Text name='topicPrefix'
                                    label={texts.common.topic} />
                            </fieldset>

                            <NotificationsForm.Settings field='topicSettings' disabled={upserting} />

                            <FormError error={upsertingError} />
                        </ModalBody>
                        <ModalFooter className='justify-content-between'>
                            <Button type='button' color='' onClick={doCloseForm} disabled={upserting}>
                                {texts.common.cancel}
                            </Button>
                            <Button type='submit' color='primary' disabled={upserting}>
                                <Loader light small visible={upserting} /> {texts.common.save}
                            </Button>
                        </ModalFooter>
                    </Form>
                )}
            </Formik>
        </Modal>
    );
};
