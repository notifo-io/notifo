/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';
import { Types } from './types';

// tslint:disable: no-parameter-reassignment

export interface ErrorDto {
    readonly statusCode: number;
    readonly response: any;
    readonly wellFormed: boolean;
}

export function isErrorVisible(error: string | ErrorDto | undefined | null, touched: boolean, submitCount: number): boolean {
    return !!error && (touched || submitCount > 0);
}

export function isError(error: any): error is ErrorDto {
    return error && Types.isNumber(error.statusCode) && error.response && Types.isBoolean(error.wellFormed);
}

export function buildErrorWithFallback(error: any, message: string): ErrorDto {
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

export function buildError(statusCode: number, response: any, wellFormed = false) {
    if (statusCode === 16) {
        statusCode = 401;
    } else if (!statusCode) {
        statusCode = 500;
    }

    return { statusCode, response, wellFormed };
}

export function formatError(error: ErrorDto) {
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
