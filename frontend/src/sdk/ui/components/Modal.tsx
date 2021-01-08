/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h, VNode } from 'preact';

import { useEffect, useRef } from 'preact/hooks';
import { Icon } from './Icon';

export interface ModalProps {
    // The children.
    children: VNode<any>[] | VNode;

    // The modal position.
    position?: 'bottom-left' | 'bottom-right';

    // Triggered when clicked outside.
    onClickOutside?: () => void;
}

export const Modal = (props: ModalProps) => {
    const { children, onClickOutside, position } = props;

    const modal = useRef<HTMLDivElement>();

    useEffect(() => {
        const handler = (event: MouseEvent) => {
            if (modal.current && event.target['parentNode'] && !modal.current.contains(event.target as any)) {
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
        <div class={buildRootClass(position)} ref={modal}>
            <div class='notifo-modal-inner'>
                {children}

                <a class='notifo-powered' href='https://notifo.io' target='_blank'>
                    <span>Powered by</span> <Icon type='logo' size={14} />
                </a>
            </div>
        </div>
    );
};

function buildRootClass(position: string) {
    let rootClazz = 'notifo-modal';

    if (position === 'bottom-right') {
        rootClazz += ' notifo-modal-right';
    } else if (position === 'bottom-left') {
        rootClazz += ' notifo-modal-left';
    }

    return rootClazz;
}
