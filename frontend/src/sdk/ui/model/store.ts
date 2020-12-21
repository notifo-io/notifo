/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

export type Dispatch<TAction> = (action: TAction) => void;

export type Recucer<TState, TAction> = (state: TState, action: TAction) => TState;

export type Listener<TState> = (state: TState) => void;

export class Store<TState, TAction> {
    private readonly listeners: Listener<TState>[] = [];

    public get current() {
        return this.state;
    }

    constructor(
        private state: TState,
        private readonly reducer: Recucer<TState, TAction>,
    ) {
    }

    public subscribe(callback: Listener<TState>) {
        this.listeners.push(callback);
    }

    public unsubscribe(callback: Listener<TState>) {
        this.listeners.splice(this.listeners.indexOf(callback), 1);
    }

    public dispatch = (action: TAction) => {
        const newState = this.reducer(this.state, action);

        if (newState !== this.state) {
            for (const listener of this.listeners) {
                listener(newState);
            }

            this.state = newState;
        }
    }
}

export function set<T>(states: { [key: string]: T }, id: string, value: T) {
    if (states[id] !== value) {
        const updated = { ...states };

        updated[id] = value;

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
