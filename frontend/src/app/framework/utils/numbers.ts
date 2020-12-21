/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: one-variable-per-declaration
// tslint:disable: prefer-const
// tslint:disable: no-parameter-reassignment

export module Numbers {
    export function formatNumber(value: number) {
        let u = 0, s = 1000;

        while (value >= s || -value >= s) {
            value /= s;
            u++;
        }

        return ((u ? `${value.toFixed(1)} ` : value) + ' kMGTPEZY'[u]).trim();
    }
}
