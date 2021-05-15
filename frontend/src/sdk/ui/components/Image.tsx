/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';

export interface ImageProps {
    class: string;

    // The size of the icon.
    src?: string;
}

export const Image = (props: ImageProps) => {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(false);
    }, [props.src]);

    const setLoaded = useCallback(() => {
        setIsLoaded(true);
    }, []);

    const style = isLoaded ? { display: 'block' } : { display: 'none' };

    return (
        <div class={props.class} style={style}>
            <img src={props.src} onLoad={setLoaded} />
        </div>
    );
};
