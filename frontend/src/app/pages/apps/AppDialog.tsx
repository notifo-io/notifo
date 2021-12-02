/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader } from '@app/framework';
import { createApp, CreateAppParams, createAppReset, useApps } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as Yup from 'yup';

const FormSchema = Yup.object().shape({
    // Required app name
    name: Yup.string()
        .label(texts.common.name).requiredI18n(),
});

export interface AppDialogProps {
    // Invoked when closed.
    onClose?: () => void;
}

export const AppDialog = (props: AppDialogProps) => {
    const { onClose } = props;

    const dispatch = useDispatch();
    const creating = useApps(x => x.creating);
    const creatingError = useApps(x => x.creatingError);
    const [wasCreating, setWasCreating] = React.useState(false);

    React.useEffect(() => {
        dispatch(createAppReset());
    }, [dispatch]);

    React.useEffect(() => {
        if (creating) {
            setWasCreating(true);
        }
    }, [creating]);

    React.useEffect(() => {
        if (!creating && wasCreating && !creatingError && onClose) {
            onClose();
        }
    }, [creating, creatingError, onClose, wasCreating]);

    const doCloseForm = React.useCallback(() => {
        onClose && onClose();
    }, [onClose]);

    const doSave = React.useCallback((params: CreateAppParams) => {
        dispatch(createApp({ params }));
    }, [dispatch]);

    const initialValues: any = {};

    return (
        <Modal isOpen={true} toggle={doCloseForm}>
            <Formik<CreateAppParams> initialValues={initialValues} onSubmit={doSave} enableReinitialize validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        <ModalHeader toggle={doCloseForm}>
                            {texts.apps.createHeader}
                        </ModalHeader>

                        <ModalBody>
                            <fieldset disabled={creating}>
                                <Forms.Text name='name' vertical
                                    label={texts.common.name} />
                            </fieldset>

                            <FormError error={creatingError} />
                        </ModalBody>
                        <ModalFooter className='justify-content-between'>
                            <Button type='button' color='none' onClick={doCloseForm}>
                                {texts.common.cancel}
                            </Button>
                            <Button type='submit' color='primary'>
                                <Loader light small visible={creating} /> {texts.common.create}
                            </Button>
                        </ModalFooter>
                    </Form>
                )}
            </Formik>
        </Modal>
    );
};
