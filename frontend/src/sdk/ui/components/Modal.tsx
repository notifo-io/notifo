/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { h, VNode } from 'preact';

import { useEffect, useRef } from 'preact/hooks';
import { Icon } from './Icon';

type ModalPosition = 'bottom-left' | 'bottom-right' | 'top-global';

export interface ModalProps {
    // The children.
    children: VNode<any>[] | VNode | null;

    // The modal position.
    position: ModalPosition;

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

export const Modal = (props: ModalProps) => {
    const { children, onClickOutside, position } = props;

    const modal = useRef<HTMLDivElement>();

    useEffect(() => {
        const handler = (event: MouseEvent) => {
            if (modal.current && event.target?.['parentNode'] && !modal.current.contains(event.target as any)) {
                onClickOutside && onClickOutside();
            }
        };

        setTimeout(() => {
            document.addEventListener('click', handler);
        });

        return () => {
            document.removeEventListener('click', handler);
        };
    }, [onClickOutside]);

    return (
        <div class={buildRootClass(position)}>
            <div class='notifo-modal-panel' ref={modal}>
                <div class='notifo-modal-inner'>
                    {children}

                    <a class='notifo-powered' href='https://notifo.io' target='_blank'>
                        <span>Powered by</span> <Icon type='logo' size={14} />
                    </a>
                </div>
            </div>
        </div>
    );
};

function buildRootClass(position: ModalPosition) {
    let rootClazz = 'notifo-modal';

    if (position === 'bottom-right') {
        rootClazz += ' notifo-modal-right';
    } else if (position === 'bottom-left') {
        rootClazz += ' notifo-modal-left';
    } else if (position === 'top-global') {
        rootClazz += ' notifo-modal-top';
    }

    return rootClazz;
}
