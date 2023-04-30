/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import { Forms } from '@app/shared/components';
import { texts } from '@app/texts';

export interface EmailTemplateMoreDialogProps {
    // Invoked when closed.
    onClose: () => void;
}

export const EmailTemplateMoreDialog = (props: EmailTemplateMoreDialogProps) => {
    const { onClose } = props;

    return (
        <Modal isOpen={true} toggle={onClose}>
            <ModalHeader toggle={onClose}>
                {texts.common.settings}
            </ModalHeader>

            <ModalBody>
                <fieldset className='mt-3'>
                    <Forms.Text name='fromEmail' vertical
                        label={texts.common.fromEmail} />

                    <Forms.Text name='fromName' vertical
                        label={texts.common.fromName} />
                </fieldset>
            </ModalBody>
            <ModalFooter className='justify-content-between'>
                <Button type='button' color='none' onClick={onClose}>
                    {texts.common.close}
                </Button>
            </ModalFooter>
        </Modal>
    );
};
