/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto, isErrorVisible } from '@app/framework/utils';
import * as React from 'react';

export interface FormControlErrorProps {
    // The list of errors.
    error?: ErrorDto | string;

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

    let clazz = 'errors';

    if (alignment) {
        clazz += ` errors-${alignment}`;
    }

    return (
        <div className='errors-container'>
            <div className={clazz}>
                {error}
            </div>
        </div>
    );
};
