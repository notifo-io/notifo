/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

export module Numbers {
    export function formatNumber(value: number) {
        // eslint-disable-next-line
        let u = 0, s = 1000;

        while (value >= s || -value >= s) {
            value /= s;
            u++;
        }

        return ((u ? `${value.toFixed(1)} ` : value) + ' kMGTPEZY'[u]).trim();
    }

    export function guid(): string {
        return `${s4() + s4()}-${s4()}-${s4()}-${s4()}-${s4()}${s4()}${s4()}`;
    }

    export function s4(): string {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }
}
