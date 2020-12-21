/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader } from '@app/framework';
import { UpsertUserDto, UserDto } from '@app/service';
import { NotificationsForm } from '@app/shared/components';
import { upsertUserAsync, useApps, useCore, useUsers } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';

export interface UserDialogProps {
    // The user to edit.
    user?: UserDto;

    // Invoked when closed.
    onClose?: () => void;
}

export const UserDialog = (props: UserDialogProps) => {
    const { onClose, user } = props;

    const dispatch = useDispatch();
    const appId = useApps(x => x.appId);
    const coreLanguages = useCore(x => x.languages);
    const coreTimezones = useCore(x => x.timezones);
    const upserting = useUsers(x => x.upserting);
    const upsertingError = useUsers(x => x.upsertingError);
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

    const doSave = React.useCallback((values: UpsertUserDto) => {
        dispatch(upsertUserAsync(appId, values));
    }, [appId]);

    const initialValues: any = user || {};

    return (
        <Modal isOpen={true} size='lg' backdrop={false} toggle={doCloseForm}>
            <Formik<UpsertUserDto> initialValues={initialValues} enableReinitialize onSubmit={doSave}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {user ? texts.users.editHeader : texts.users.createHeader}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset disabled={upserting}>
                                <Forms.Text name='id'
                                    label={texts.common.id} />

                                <Forms.Text name='fullName'
                                    label={texts.common.name} />

                                <Forms.Text name='emailAddress'
                                    label={texts.common.emailAddress} />

                                <Forms.Text name='phoneNumber'
                                    label={texts.common.phoneNumber} />

                                <Forms.Select name='preferredLanguage' options={coreLanguages}
                                    label={texts.common.language} />

                                <Forms.Select name='preferredTimezone' options={coreTimezones}
                                    label={texts.common.timezone} />
                            </fieldset>

                            <NotificationsForm.Settings field='settings' disabled={upserting} />

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
