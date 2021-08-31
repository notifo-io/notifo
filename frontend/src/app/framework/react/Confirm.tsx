/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';
import * as React from 'react';
import { Button, Modal, ModalBody, ModalFooter } from 'reactstrap';
import { Types } from '../utils';
import { useDialog } from './hooks';

export interface ConfirmProps {
    // The confirm title.
    title?: string;

    // The confirm text.
    text: string;

    // The confirm delegate.
    onConfirm: () => void;

    // The children.
    children: ((props: { onClick: (event: any) => void }) => React.ReactNode);
}

export const Confirm = (props: ConfirmProps) => {
    const { children, onConfirm, text, title } = props;

    const dialog = useDialog();

    const doConfirm = React.useCallback(() => {
        onConfirm();

        dialog.close();
    }, [dialog, onConfirm]);

    const doRender = React.useCallback(() => {
        const onClick = (event: any) => {
            dialog.open();

            if (Types.isFunction(event.stopPropagation)) {
                event.stopPropagation();
            }

            return false;
        };

        return children({ onClick });
    }, [children, dialog]);

    return (
        <>
            {doRender()}

            {dialog.isOpen &&
                <Modal size='sm' isOpen={true}>
                    <ModalBody>
                        {title &&
                            <h4>{title}</h4>
                        }

                        <div>{text}</div>
                    </ModalBody>
                    <ModalFooter className='justify-content-between'>
                        <Button color='danger' outline onClick={dialog.close}>
                            {texts.common.no}
                        </Button>
                        <Button color='success' onClick={doConfirm}>
                            {texts.common.yes}
                        </Button>
                    </ModalFooter>
                </Modal>
            }
        </>
    );
};
