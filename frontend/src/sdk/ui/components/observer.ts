/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { useEffect, useState } from 'preact/hooks';

const IS_SUPPORTED = !!window['IntersectionObserver'];

let observer: IntersectionObserver;
let callbacks: { element: HTMLElement, callback: (value: number) => void }[] = [];

function init(parent: HTMLElement) {
    if (!observer || observer.root !== parent) {
        callbacks = [];

        observer = new IntersectionObserver(elements => {
            elements.forEach(element => {
                for (const { callback } of callbacks.filter(x => x.element === element.target)) {
                    callback(element.intersectionRatio);
                }
            });
        }, { root: parent });
    }
}

function observe(element: HTMLElement, parent: HTMLElement, callback: (value: number) => void) {
    init(parent);

    const currentObserver = observer;

    callbacks.push({ element, callback });

    currentObserver.observe(element);

    return () => {
        for (let i = 0; i < callbacks.length; i++) {
            if (callbacks[i].callback === callback) {
                callbacks.splice(i, 1);
                break;
            }
        }

        currentObserver.unobserve(element);
    };
}

export function useInView(element: HTMLElement | null | undefined, parent: HTMLElement | null | undefined) {
    const [isInView, setIsInView] = useState(false);

    useEffect(() => {
        if (IS_SUPPORTED && element && parent) {
            return observe(element, parent, ratio => {
                if (ratio > 0) {
                    setIsInView(true);
                }
            });
        }

        return;
    }, [element, parent]);

    return isInView;
}
