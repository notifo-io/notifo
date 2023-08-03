/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable no-console */

type QueueItem = () => Promise<any>;

export class JobQueue {
    private readonly queue: QueueItem[] = [];
    private running?: QueueItem;

    public enqueue(item: QueueItem) {
        if (!this.running) {
            this.execute(item);
        } else {
            this.queue.push(item);
        }
    }

    private execute(item: QueueItem) {
        this.running = item;

        item().finally(() => {
            this.running = undefined;

            this.handleNext();
        });
    }

    private handleNext() {
        const next = this.queue[0];

        if (next) {
            this.queue.splice(0, 1);

            this.execute(next);
        }
    }
}

export function isArray(value: any): value is any[] {
    return Array.isArray(value);
}

export function isString(value: any): value is string {
    return typeof value === 'string' || value instanceof String;
}

export function isUndefined(value: any): value is undefined {
    return typeof value === 'undefined';
}

export function isBoolean(value: any): value is boolean {
    return typeof value === 'boolean';
}

export function isFunction(value: any): value is Function {
    return typeof value === 'function';
}

export function isNumber(value: any): value is number {
    return typeof value === 'number' && Number.isFinite(value);
}

export function isObject(value: any): value is Object {
    return value && typeof value === 'object' && value.constructor === Object;
}

export function logWarn(message: string) {
    console.warn(`NOTIFO SDK: ${message}`);
}

export function logError(message: string) {
    console.error(`NOTIFO SDK: ${message}`);
}

export function withPreset(src: string | undefined | null, preset: string) {
    if (!src) {
        return undefined;
    }

    if (src.indexOf('?') >= 0) {
        return `${src}&preset=${preset}`;
    } else {
        return `${src}?preset=${preset}`;
    }
}

export function combineUrl(baseUrl: string, relativeUrl: string) {
    let b = baseUrl;

    if (b.endsWith('/')) {
        b = b.substring(0, b.length - 1);
    }

    let r = relativeUrl;

    if (r.startsWith('/')) {
        r = r.substring(1);
    }

    return `${b}/${r}`;
}

export function getHostName(url: string) {
    try {
        return new URL(url).host;
    } catch {
        // URL is very likely relative.
        return undefined;
    }
}

export function delayTime(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

const styles: { [url: string]: Promise<boolean> } = {};

export function loadStyle(url: string) {
    let promise = styles[url];

    if (promise) {
        return promise;
    }

    promise = new Promise((resolve, reject) => {
        const styleElement = document.createElement('link');

        styleElement.rel = 'stylesheet';
        styleElement.href = url;
        styleElement.type = 'text/css';

        styleElement.addEventListener('load', () => {
            resolve(true);
        });

        styleElement.addEventListener('error', () => {
            reject();
        });

        document.head.appendChild(styleElement);
    });

    styles[url] = promise;

    return promise;
}