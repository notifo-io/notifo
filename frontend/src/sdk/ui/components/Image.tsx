/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';

export interface ImageProps {
    className: string;

    // The size of the icon.
    src: string;
}

export const Image = ({ src, className }: ImageProps) => {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(false);
    }, [src]);

    const setLoaded = useCallback(() => {
        setIsLoaded(true);
    }, []);

    const style = isLoaded ? {} : { display: 'none' };

    return (
        <div class={className} style={style}>
            <img src={src} onLoad={setLoaded} />
        </div>
    );
};
