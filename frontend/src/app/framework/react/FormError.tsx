/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, formatError, isError, Types } from '@app/framework/utils';
import * as React from 'react';
import { Alert } from 'reactstrap';

export interface FormErrorProps {
    // The error.
    error?: ErrorDto | string;

    // Optional class name.
    className?: string;

    // Optional size.
    size?: 'sm';
}

export const FormError = (props: FormErrorProps) => {
    const { className, error, size } = props;

    let errorString: string;

    if (Types.isString(error)) {
        errorString = error;
    } else if (isError(error)) {
        errorString = formatError(error);
    }

    if (!errorString) {
        return null;
    }

    let clazz = 'fade';

    if (size) {
        clazz += ' ';
        clazz += `alert-${size}`;
    }

    if (className) {
        clazz += ' ';
        clazz += className;
    }

    return <Alert className={clazz} color='danger'>{errorString}</Alert>;
};
