/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { formatDistanceToNow } from 'date-fns';
import * as React from 'react';
import { Types } from './../utils';

export interface FromNowProps {
    date: Date | number | string;
}

export const FromNow = React.memo((props: FromNowProps) => {
    let { date } = props;

    if (Types.isNumber(date)) {
        date *= 1000;
    }

    if (Types.isString(date)) {
        date = Date.parse(date);
    }

    return (
        <>
            {formatDistanceToNow(date, { addSuffix: true })}
        </>
    );
});
