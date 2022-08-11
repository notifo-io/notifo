/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as Yup from 'yup';
import { FormError, Loader, Types } from '@app/framework';
import { SubscriptionDto } from '@app/service';
import { Forms, NotificationsForm } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { upsertSubscription, useApp, useSubscriptions } from '@app/state';
import { texts } from '@app/texts';

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
    const app = useApp()!;
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
        if (!upserting && wasUpserting && !upsertingError && onClose) {
            onClose();
        }
    }, [onClose, upserting, upsertingError, wasUpserting]);

    const doCloseForm = React.useCallback(() => {
        if (onClose) {
            onClose();
        }
    }, [onClose]);

    const doSave = React.useCallback((params: SubscriptionDto) => {
        dispatch(upsertSubscription({ appId, userId, params }));
    }, [dispatch, appId, userId]);

    const initialValues: any = React.useMemo(() => {
        const result: Partial<SubscriptionDto> = Types.clone(subscription || { topicPrefix: '' });

        result.topicSettings ||= {};

        for (const channel of CHANNELS) {
            result.topicSettings[channel] ||= { send: 'Inherit', condition: 'Inherit' };
        }

        return result;
    }, [subscription]);

    return (
        <Modal isOpen={true} size='lg' toggle={doCloseForm}>
            <Formik<SubscriptionDto> initialValues={initialValues} enableReinitialize onSubmit={doSave} validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {subscription ? texts.subscriptions.editHeader : texts.subscriptions.createHeader}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset className='mt-3' disabled={upserting}>
                                <Forms.Text name='topicPrefix'
                                    label={texts.common.topic} />
                            </fieldset>

                            <NotificationsForm.Settings field='topicSettings' disabled={upserting} />

                            <FormError error={upsertingError} />
                        </ModalBody>
                        <ModalFooter className='justify-content-between'>
                            <Button type='button' color='none' onClick={doCloseForm} disabled={upserting}>
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
