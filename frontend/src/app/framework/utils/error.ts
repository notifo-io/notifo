/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';
import { Types } from './types';

// tslint:disable: no-parameter-reassignment

export interface ErrorInfo {
    readonly statusCode: number;
    readonly response: any;
    readonly details?: ErrorDetails[];
    readonly wellFormed: boolean;
}

export interface ErrorDetails {
    readonly message: string;
}

export function isErrorVisible(error: string | ErrorInfo | undefined | null, touched: boolean, submitCount: number): boolean {
    return !!error && (touched || submitCount > 0);
}

export function isError(error: any): error is ErrorInfo {
    return error && Types.isNumber(error.statusCode) && Types.isBoolean(error.wellFormed);
}

export function buildErrorWithFallback(error: any, message: string): ErrorInfo {
    if (isError(error)) {
        if (error.wellFormed) {
            return error;
        } else {
            return buildError(error.statusCode, message, true);
        }
    } else {
        return buildError(500, message);
    }
}

export function buildError(statusCode: number, response: any, details?: any, wellFormed = false) {
    if (statusCode === 16) {
        statusCode = 401;
    } else if (!statusCode) {
        statusCode = 500;
    }

    if (Types.isArrayOfString(details)) {
        details = details.map(message => ({ message }));
    } else {
        details = undefined;
    }

    return { statusCode, response, details, wellFormed };
}

export function formatError(error: ErrorInfo) {
    if (error.response) {
        if (Types.isString(error.response)) {
            return error.response;
        } else if (Types.isString(error.response.message)) {
            return error.response.message;
        }
    }

    return texts.common.error;
}

export function extractValue(value: any, path: string) {
    const parts = path.split('[').join('.').split(']').join('.').split('.');

    let result = value;

    for (const part of parts) {
        if (part) {
            if (Types.isArray(result)) {
                result = result[parseInt(part, 10)];
            } else if (Types.isObject(result)) {
                result = result[part];
            } else {
                return undefined;
            }
        }
    }

    return result;
}
