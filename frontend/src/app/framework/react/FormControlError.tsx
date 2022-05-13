/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { ErrorInfo, formatError, isError, isErrorVisible, Types } from '@app/framework/utils';

export interface FormControlErrorProps {
    // The list of errors.
    error?: ErrorInfo | string;

    // The submit count.
    submitCount: number;

    // True when the control is touched.
    touched: boolean;

    // The alignment.
    alignment?: 'left' | 'right';
}

export const FormControlError = (props: FormControlErrorProps) => {
    const { alignment, error, submitCount, touched } = props;

    if (!isErrorVisible(error, touched, submitCount)) {
        return null;
    }

    let errorString: string | undefined;

    if (Types.isString(error)) {
        errorString = error;
    } else if (isError(error)) {
        errorString = formatError(error);
    }

    return (
        <div className='errors-container'>
            <div className={classNames('errors', alignment ? `errors-${alignment}` : false)}>
                {errorString}
            </div>
        </div>
    );
};
