/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { isFunction } from '@sdk/shared';

export type Dispatch<TAction> = (action: TAction) => void;
export type StoreRecucer<TState, TAction> = (state: TState, action: TAction) => TState;
export type StoreListener<TState> = (state: TState) => void;

export class Store<TState, TAction extends { type: string }> {
    private readonly listeners: StoreListener<TState>[] = [];
    private devTools: any;

    public get current() {
        return this.state;
    }

    constructor(
        private state: TState,
        private readonly reducer: StoreRecucer<TState, TAction>,
    ) {
        const extension = window['__REDUX_DEVTOOLS_EXTENSION__'];

        if (extension) {
            this.devTools = extension.connect({
                name: window['title'] ? `${window['title']} Notifo SDK` : 'Notifo SDK',
            });

            this.devTools.init(state);
        }
    }

    public destroy() {
        this.devTools?.disconnect();
    }

    public subscribe(callback: StoreListener<TState>) {
        this.listeners.push(callback);
    }

    public unsubscribe(callback: StoreListener<TState>) {
        this.listeners.splice(this.listeners.indexOf(callback), 1);
    }

    public dispatch = (action: TAction) => {
        const newState = this.reducer(this.state, action);

        if (this.devTools) {
            this.devTools.send(action, newState);
        }

        this.setState(newState);
    }

    private setState(newState: any) {
        if (newState !== this.state) {
            for (const listener of this.listeners) {
                listener(newState);
            }

            this.state = newState;
        }
    }
}

export function set<T>(states: { [key: string]: T }, id: string, update: T | ((previous: T) => T)) {
    let newValue: T;

    if (isFunction(update)) {
        newValue = update(states[id]);
    } else {
        newValue = update;
    }

    if (states[id] !== newValue) {
        const updated = { ...states };

        updated[id] = newValue;

        return updated;
    }

    return states;
}

export function remove<T>(states: { [key: string]: T }, id: string) {
    if (states[id]) {
        const updated = { ...states };

        delete updated[id];

        return updated;
    }

    return states;
}
