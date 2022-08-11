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
import { SystemUserDto, UpdateSystemUserDto } from '@app/service';
import { Forms } from '@app/shared/components';
import { upsertSystemUser, useSystemUsers } from '@app/state';
import { texts } from '@app/texts';

const FormSchema = Yup.object({
    // Required email
    email: Yup.string()
        .label(texts.common.email).requiredI18n().emailI18n(),

    // The mode (template or formatted).
    passwordCompare: Yup.string()
        .label(texts.common.passwordConfirm).oneOf([Yup.ref('password'), '', undefined], texts.common.passwordsDoNotMatch),
});

export interface SystemUserDialogProps {
    // The system user to edit.
    user?: SystemUserDto;

    // Invoked when closed.
    onClose?: () => void;
}

const ROLES = [{
    label: 'Admin',
    value: 'ADMIN',
}];

export const SystemUserDialog = (props: SystemUserDialogProps) => {
    const { onClose, user } = props;

    const dispatch = useDispatch();
    const upserting = useSystemUsers(x => x.upserting);
    const upsertingError = useSystemUsers(x => x.upsertingError);
    const [wasUpserting, setWasUpserting] = React.useState(false);

    React.useEffect(() => {
        if (upserting) {
            setWasUpserting(true);
        }
    }, [upserting]);

    const doCloseForm = React.useCallback(() => {
        if (onClose) {
            onClose();
        }
    }, [onClose]);

    React.useEffect(() => {
        if (!upserting && wasUpserting && !upsertingError) {
            doCloseForm();
        }
    }, [dispatch, doCloseForm, upserting, upsertingError, wasUpserting]);

    const doSave = React.useCallback((params: UpdateSystemUserDto) => {
        dispatch(upsertSystemUser({ userId: user?.id, params }));
    }, [dispatch, user?.id]);

    const initialValues: any = React.useMemo(() => {
        const result: Partial<UpdateSystemUserDto> = Types.clone(user || { roles: [] });

        return result;
    }, [user]);

    return (
        <Modal isOpen={true} size='lg' backdrop={false} toggle={doCloseForm}>
            <Formik<UpdateSystemUserDto> initialValues={initialValues} enableReinitialize onSubmit={doSave} validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {user ? texts.systemUsers.editHeader : texts.systemUsers.createHeader}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset className='mt-3' disabled={upserting}>
                                <Forms.Text name='email'
                                    label={texts.common.emailAddress} />
                    
                                <Forms.Checkboxes name='roles' options={ROLES}
                                    label={texts.common.roles} />
                    
                                <Forms.Password name='password'
                                    label={texts.common.password} />
                    
                                <Forms.Password name='passwordConfirm'
                                    label={texts.common.passwordConfirm} />
                            </fieldset>

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
