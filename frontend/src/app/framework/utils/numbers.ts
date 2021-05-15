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
}
