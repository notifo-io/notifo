/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';

import { useEffect, useState } from 'preact/hooks';
import { Icon } from './Icon';

export interface LoaderProps {
    // True if visible.
    visible: boolean;

    // The size in pixels.
    size: number;
}

export const Loader = (props: LoaderProps) => {
    const { size, visible } = props;

    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        if (visible) {
            setIsVisible(true);

            return undefined;
        } else {
            const timeout = setTimeout(() => {
                setIsVisible(false);
            }, 300);

            return () => clearTimeout(timeout);
        }
    }, [visible]);

    if (!isVisible) {
        return null;
    }

    return (
        <span className='notifo-loader'>
            <Icon type='loader' size={size} />
        </span>
    );
};
