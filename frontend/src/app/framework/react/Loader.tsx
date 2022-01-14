/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import ExternalLoader from 'react-loader-spinner';

export interface LoaderProps {
    // Optional class name.
    className?: string;

    // Indicates if the spinenr is visible.
    visible?: boolean | null;

    // Use small rendering.
    small?: boolean;

    // Optional text.
    text?: string;

    // True if light.
    light?: boolean;
}

export const Loader = React.memo((props: LoaderProps) => {
    const { className, light, small, text, visible } = props;

    const [isVisible, setIsVisible] = React.useState(false);

    React.useEffect(() => {
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

    const color = light ? '#fff' : '#444';

    return small ? (
        <small className={classNames(className, 'loader')}>
            <ExternalLoader width={16} height={16} type='ThreeDots' color={color} /> {text}
        </small>
    ) : (
        <span className={classNames(className, 'loader')}>
            <ExternalLoader width={26} height={26} type='ThreeDots' color={color} /> {text}
        </span>
    );
});
