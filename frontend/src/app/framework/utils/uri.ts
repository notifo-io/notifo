/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

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
