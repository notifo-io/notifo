/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { Button, Form, Modal, ModalBody, ModalFooter, ModalHeader, Nav, NavItem, NavLink } from 'reactstrap';
import { FormError, Loader, Types, useEventCallback } from '@app/framework';
import { Clients, UpsertUserDto, UserDto } from '@app/service';
import { Forms, NotificationsForm } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { upsertUser, useApp, useCore, useUsers } from '@app/state';
import { texts } from '@app/texts';

export interface UserDialogProps {
    // The user to edit.
    user?: UserDto;

    // Invoked when closed.
    onClose?: () => void;
}

export const UserDialog = (props: UserDialogProps) => {
    const { onClose } = props;

    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const coreLanguages = useCore(x => x.languages);
    const coreTimezones = useCore(x => x.timezones);
    const upserting = useUsers(x => x.upserting);
    const upsertingError = useUsers(x => x.upsertingError);
    const [dialogUser, setDialogUser] = React.useState(props.user);
    const [dialogTab, setDialogTab] = React.useState(0);
    const [wasUpserting, setWasUpserting] = React.useState(false);
    const dialogUserId = dialogUser?.id;

    React.useEffect(() => {
        async function loadData(id: string) {
            try {
                const newUser = await Clients.Users.getUser(appId, id);

                setDialogUser(newUser);
            } catch {
            }
        }

        if (dialogUserId) {
            loadData(dialogUserId);
        }
    }, [appId, dialogUserId]);

    React.useEffect(() => {
        if (upserting) {
            setWasUpserting(true);
        }
    }, [upserting]);

    React.useEffect(() => {
        if (!upserting && wasUpserting && !upsertingError) {
            onClose && onClose();
        }
    }, [dispatch, onClose, upserting, upsertingError, wasUpserting]);

    const doSave = useEventCallback((params: UpsertUserDto) => {
        dispatch(upsertUser({ appId, params }));
    });

    const allProperties = React.useMemo(() => {
        const properties = [...dialogUser?.userProperties || []];

        if (dialogUser?.properties) {
            for (const name of Object.keys(dialogUser.properties)) {
                if (!properties.find(x => x.name === name)) {
                    properties.push({ name, editorLabel: name });
                }
            }
        }

        return properties.sortByString(x => x.name);
    }, [dialogUser]);

    const defaultValues: any = React.useMemo(() => {
        const result: Partial<UserDto> = Types.clone(dialogUser || {});

        result.settings ||= {};

        for (const channel of CHANNELS) {
            result.settings[channel] ||= { send: 'Inherit', condition: 'Inherit' };
        }

        return result;
    }, [dialogUser]);

    const form = useForm<UpsertUserDto>({ defaultValues, mode: 'onChange' });

    return (
        <Modal isOpen={true} size='lg' backdrop={false} toggle={onClose}>
            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doSave)}>
                    <ModalHeader toggle={onClose}>
                        &nbsp;
                        
                        <Nav className='nav-tabs2'>
                            <NavItem>
                                <NavLink onClick={() => setDialogTab(0)} active={dialogTab === 0}>{dialogUser ? texts.users.editHeader : texts.users.createHeader}</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink onClick={() => setDialogTab(1)} active={dialogTab === 1}>{texts.common.channels}</NavLink>
                            </NavItem>
                        </Nav>
                    </ModalHeader>

                    <ModalBody>
                        <fieldset className='mt-3' disabled={upserting}>
                            {dialogTab === 0 ? (
                                <>
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

                                    {allProperties.length > 0 &&
                                        <>
                                            <hr />

                                            {allProperties.map(x =>
                                                <Forms.Text key={x.name} name={x.name} hints={x.editorDescription}
                                                    label={x.editorLabel || x.name} />,
                                            )}
                                        </>
                                    }
                                </>
                            ) : (
                                <NotificationsForm.Settings field='settings' disabled={upserting} />
                            )}
                        </fieldset>

                        <FormError error={upsertingError} />
                    </ModalBody>
                    <ModalFooter className='justify-content-between'>
                        <Button type='button' color='none' onClick={onClose} disabled={upserting}>
                            {texts.common.cancel}
                        </Button>
                        <Button type='submit' color='primary' disabled={upserting}>
                            <Loader light small visible={upserting} /> {texts.common.save}
                        </Button>
                    </ModalFooter>
                </Form>
            </FormProvider>
        </Modal>
    );
};
