import { ErrorDto, FetchError, RequiredError, ResponseError } from "../generated";

export class NotifoError<T = unknown> extends Error {
    constructor(
        public readonly statusCode?: number,
        public readonly body?: T,
        public readonly cause?: Error,
        message?: string,
    ) {
        super(buildMessage(statusCode, body, message, cause));

        Object.setPrototypeOf(this, NotifoError.prototype);

        if (statusCode != null) {
            this.statusCode = statusCode;
        }

        if (body !== undefined) {
            this.body = body;
        }
    }
}

export class NotifoBadRequestError extends NotifoError<ErrorDto> {
    constructor(body?: ErrorDto, cause?: ResponseError) {
        super(400, body, cause);
        Object.setPrototypeOf(this, NotifoBadRequestError.prototype);
    }
}

export class NotifoForbiddenError extends NotifoError<ErrorDto> {
    constructor(body?: ErrorDto, cause?: ResponseError) {
        super(403, body, cause);
        Object.setPrototypeOf(this, NotifoForbiddenError.prototype);
    }
}

export class NotifoUnauthorizedError extends NotifoError<ErrorDto> {
    constructor(body?: ErrorDto, cause?: ResponseError, message?: string) {
        super(401, body, cause, message);
        Object.setPrototypeOf(this, NotifoUnauthorizedError.prototype);
    }
}

export class NotifoConflictError extends NotifoError<ErrorDto> {
    constructor(body: ErrorDto, cause: ResponseError) {
        super(409, body, cause);
        Object.setPrototypeOf(this, NotifoConflictError.prototype);
    }
}

export class NotifoContentTooLargeError extends NotifoError<ErrorDto> {
    constructor(body?: ErrorDto, cause?: ResponseError) {
        super(413, body, cause);
        Object.setPrototypeOf(this, NotifoContentTooLargeError.prototype);
    }
}

export class NotifoInternalServerError extends NotifoError<ErrorDto> {
    constructor(body?: ErrorDto, cause?: ResponseError) {
        super(500, body, cause, cause?.message);
        Object.setPrototypeOf(this, NotifoInternalServerError.prototype);
    }
}

export class NotifoNotFoundError extends NotifoError {
    constructor(cause?: ResponseError) {
        super(404, undefined, cause);
        Object.setPrototypeOf(this, NotifoNotFoundError.prototype);
    }
}

export class NotifoRequiredFieldError extends NotifoError {
    constructor(cause?: RequiredError) {
        super(undefined, undefined, cause);
        Object.setPrototypeOf(this, NotifoNotFoundError.prototype);
    }
}

export async function buildError(error: unknown) {
    if (error instanceof FetchError) {
        return new NotifoError(undefined, undefined, error, error.message);
    } else if (error instanceof RequiredError) {
        return new NotifoRequiredFieldError(error);
    } else if (error instanceof ResponseError) {
        const statusCode = error.response.status;

        let body: ErrorDto;
        try {
            body = await error.response.json();
        } catch {
            body = { message: "No error details provided.", statusCode };
        }

        switch (error.response.status) {
            case 400:
                return new NotifoBadRequestError(body, error);
            case 401:
                return new NotifoUnauthorizedError(body, error);
            case 403:
                return new NotifoForbiddenError(body, error);
            case 404:
                return new NotifoNotFoundError(error);
            case 409:
                return new NotifoConflictError(body, error);
            case 413:
                return new NotifoContentTooLargeError(body, error);
            default:
                return new NotifoError(statusCode, body, error, `Exception failed with status code ${statusCode}.`);
        }
    } else {
        return new NotifoError(undefined, undefined, error as any);
    }
}

function buildMessage(statusCode?: number, body?: unknown, message?: string, cause?: Error): string {
    const lines: string[] = [];
    if (message) {
        lines.push(message);
    }

    if (statusCode) {
        lines.push(`Status code: ${statusCode.toString()}`);
    }

    if (body) {
        lines.push(`Body: ${JSON.stringify(body, undefined, 2)}`);
    }

    if (cause) {
        lines.push(`Inner: ${cause}`);
    }

    return lines.join("\n");
}
