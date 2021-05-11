/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDetails, ErrorDto, formatError, isError, Types } from '@app/framework/utils';
import * as React from 'react';
import { Alert } from 'reactstrap';

export interface FormErrorProps {
    // The error.
    error?: ErrorDto | string | null;

    // Optional class name.
    className?: string;

    // Optional size.
    size?: 'sm';
}

export const FormError = (props: FormErrorProps) => {
    const { className, error, size } = props;

    if (error && error['statusCode'] === 401) {
        return null;
    }

    let errorDetails: ReadonlyArray<ErrorDetails> | undefined;
    let errorString: string | undefined;

    if (Types.isString(error)) {
        errorString = error;
    } else if (isError(error)) {
        errorString = formatError(error);
        errorDetails = error.details;
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

    return (
        <Alert className={clazz} color='danger'>
            <>
                {errorString}

                {errorDetails && errorDetails.length > 0 &&
                    <ul>
                        {errorDetails.map((x, i) => (
                            <li key={i}>{x.message}</li>
                        ))}
                    </ul>
                }
            </>
        </Alert>
    );
};
