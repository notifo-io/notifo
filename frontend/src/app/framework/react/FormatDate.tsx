/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { formatDistanceToNow, format as formatText } from 'date-fns';
import * as React from 'react';
import { Types } from './../utils';

export interface FormatDateProps {
    // The date value.
    date?: Date | number | string;

    // The format.
    format?: string;
}

export const FormatDate = React.memo((props: FormatDateProps) => {
    const { date, format } = props;

    if (!date) {
        return null;
    }

    let dateValue: Date | number | string = date;

    if (Types.isNumber(dateValue)) {
        dateValue *= 1000;
    }

    if (Types.isString(dateValue)) {
        dateValue = Date.parse(dateValue);
    }

    const text = format
        ? formatText(dateValue, format)
        : formatDistanceToNow(dateValue, { addSuffix: true });

    return (
        <span className='truncate'>
            {text}
        </span>
    );
});
