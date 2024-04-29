/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { yupResolver } from '@hookform/resolvers/yup';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as Yup from 'yup';
import { FormAlert, FormError, Loader, useEventCallback } from '@app/framework';
import { Forms } from '@app/shared/components';
import { createApp, CreateAppParams, useApps } from '@app/state';
import { texts } from '@app/texts';

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

    const dispatch = useDispatch<any>();
    const createError = useApps(x => x.creating?.error);
    const createRunning = useApps(x => x.creating?.isRunning);
    const [wasCreating, setWasCreating] = React.useState(false);

    React.useEffect(() => {
        dispatch(createApp.reset());
    }, [dispatch]);

    React.useEffect(() => {
        if (createRunning) {
            setWasCreating(true);
        }
    }, [createRunning]);

    React.useEffect(() => {
        if (!createRunning && wasCreating && !createError && onClose) {
            onClose();
        }
    }, [createRunning, createError, onClose, wasCreating]);

    const doSave = useEventCallback((params: CreateAppParams) => {
        dispatch(createApp({ params }));
    });

    const form = useForm<CreateAppParams>({ resolver: yupResolver<any>(FormSchema), mode: 'onChange' });

    return (
        <Modal isOpen={true} toggle={onClose}>
            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doSave)}>
                    <ModalHeader toggle={onClose}>
                        {texts.apps.createHeader}
                    </ModalHeader>

                    <ModalBody>
                        <FormAlert text={texts.apps.createInfo} />

                        <fieldset className='mt-3' disabled={createRunning}>
                            <Forms.Text name='name' vertical
                                label={texts.common.name} />
                        </fieldset>

                        <FormError error={createError} />
                    </ModalBody>
                    <ModalFooter className='justify-content-between'>
                        <Button type='button' color='none' onClick={onClose}>
                            {texts.common.cancel}
                        </Button>
                        <Button type='submit' color='primary'>
                            <Loader light small visible={createRunning} /> {texts.common.create}
                        </Button>
                    </ModalFooter>
                </Form>
            </FormProvider>
        </Modal>
    );
};
