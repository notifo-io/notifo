export function getHeader(init: RequestInit, key: string) {
    if (Array.isArray(init.headers)) {
        return init.headers.find((x) => x[0] === key)?.[1];
    } else if (init.headers instanceof Headers) {
        return init.headers.get(key);
    } else if (init.headers) {
        return init.headers[key];
    } else {
        return undefined;
    }
}

export function addHeader(init: RequestInit, key: string, value: string) {
    init.headers ||= {};

    if (Array.isArray(init.headers)) {
        init.headers.push([key, value]);
    } else if (init.headers instanceof Headers) {
        init.headers.append(key, value);
    } else {
        init.headers[key] = value;
    }
}
