/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Modal, ModalBody, ModalFooter } from 'reactstrap';
import { texts } from '@app/texts';
import { Types } from './../utils';
import { useBoolean, useEventCallback } from './hooks';

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

    const [isOpen, setIsOpen] = useBoolean();

    const doConfirm = useEventCallback(() => {
        onConfirm();

        setIsOpen.on();
    });

    const doRender = React.useCallback(() => {
        const onClick = (event: any) => {
            setIsOpen.on();

            if (Types.isFunction(event.stopPropagation)) {
                event.stopPropagation();
            }

            if (Types.isFunction(event.preventDefault)) {
                event.preventDefault();
            }

            return false;
        };

        return children({ onClick });
    }, [children, setIsOpen]);

    return (
        <>
            {doRender()}

            {isOpen &&
                <Modal size='sm' isOpen={true}>
                    <ModalBody>
                        {title &&
                            <h4>{title}</h4>
                        }

                        <div>{text}</div>
                    </ModalBody>
                    <ModalFooter className='justify-content-between'>
                        <Button color='danger' outline onClick={setIsOpen.off}>
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
