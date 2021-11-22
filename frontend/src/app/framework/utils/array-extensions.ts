/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable */

interface Array<T> {
    remove(value: T): boolean;

    removeBy<TId>(field: (value: T) => TId, oldId: TId): boolean;

    set<TId>(field: (value: T) => TId, value: T): boolean;

    setOrPush<TId>(field: (value: T) => TId, value: T): void;

    setOrUnshift<TId>(field: (value: T) => TId, value: T): void;
}

Array.prototype.removeBy = function<T, TId>(field: (value: T) => TId, oldId: TId): boolean {
    for (let i = 0; i < this.length; i++) {
        if (field(this[i]) === oldId) {
            this.splice(i, 1);
            return true;
        }
    }

    return false;
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

Array.prototype.remove = function<T>(value: T): boolean {
    const index = this.indexOf(value);

    if (index >= 0) {
        this.slice(index, 1);
        return true;
    } else {
        return false;
    }
};
