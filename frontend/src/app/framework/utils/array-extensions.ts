/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable */

interface ReadonlyArray<T> {
    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removed(value?: T): ReadonlyArray<T>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;

    toMap(selector: (value: T) => string): { [key: string]: T };
}

interface Array<T> {
    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    replaceBy(field: keyof T, value: T): Array<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removeBy(field: keyof T, value: T): Array<T>;

    removeByValue<TId>(field: (value: T) => TId, oldId: TId): boolean;

    removed(value: T): ReadonlyArray<T>;

    remove(value: T): Array<T>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;

    sortByString(selector: (value: T) => string): Array<T>;

    set<TId>(field: (value: T) => TId, value: T): boolean;

    setOrPush<TId>(field: (value: T) => TId, value: T): void;

    setOrUnshift<TId>(field: (value: T) => TId, value: T): void;

    toMap(selector: (value: T) => string): { [key: string]: T };
}

Array.prototype.replaceBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field || !value) {
        return self;
    }

    for (let i = 0; i < self.length; i++) {
        const item = self[i];

        if (value[field] === item[field]) {
            self[i] = value;
            break;
        }
    }

    return self;
};

Array.prototype.replacedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field || !value) {
        return self;
    }

    const copy: T[] = [];

    for (const item of self) {
        copy.push(item);
    }

    for (let i = 0; i < self.length; i++) {
        const item = self[i];

        if (value[field] === item[field]) {
            copy[i] = value;
            break;
        }
    }

    return copy;
};

Array.prototype.removeBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field || !value) {
        return self;
    }

    self.splice(self.findIndex(x => x[field] === value[field]), 1);

    return self;
};

Array.prototype.removeByValue = function<T, TId>(field: (value: T) => TId, oldId: TId): boolean {
    for (let i = 0; i < this.length; i++) {
        if (field(this[i]) === oldId) {
            this.splice(i, 1);
            return true;
        }
    }

    return false;
};

Array.prototype.removed = function<T>(value?: T) {
    const self: ReadonlyArray<T> = this;

    if (!value) {
        return this;
    }

    return self.filter((v: T) => v !== value);
};

Array.prototype.remove = function<T>(value?: T) {
    const self: T[] = this;

    if (!value) {
        return this;
    }

    const index = self.indexOf(value);

    self.splice(index, 1);

    return self;
};

Array.prototype.removedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field || !value) {
        return self;
    }

    return self.filter((v: T) => v[field] !== value[field]);
};

Array.prototype.sorted = function() {
    const self: any[] = this;

    const copy: any[] = [];

    for (const item of self) {
        copy.push(item);
    }

    copy.sort();

    return copy;
};

Array.prototype.sortedByString = function<T>(selector: (value: T) => string) {
    const self: ReadonlyArray<any> = this;

    if (!selector) {
        return self;
    }

    const copy: T[] = [];

    for (const item of self) {
        copy.push(item);
    }

    copy.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return copy;
};

Array.prototype.sortByString = function<T>(selector: (value: T) => string) {
    const self: any[] = this;

    if (!selector) {
        return self;
    }

    self.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return self;
};

Array.prototype.toMap = function<T>(selector: (value: T) => string) {
    const result: { [key: string]: T } = {};

    for (const item of this) {
        result[selector(item)] = item;
    }

    return result;
};

Array.prototype.set = function<T, TId>(field: (value: T) => TId, newValue: T): boolean {
    if (!newValue) {
        return false;
    }

    const newId = field(newValue);

    for (let i = 0; i < this.length; i++) {
        if (field(this[i]) === newId) {
            this[i] = newValue;
            return true;
        }
    }

    return false;
};

Array.prototype.setOrPush = function<T, TId>(field: (value: T) => TId, newValue: T): void {
    if (!this.set(field, newValue)) {
        this.push(newValue);
    }
};

Array.prototype.setOrUnshift = function<T, TId>(field: (value: T) => TId, newValue: T): void {
    if (!this.set(field, newValue)) {
        this.unshift(newValue);
    }
};