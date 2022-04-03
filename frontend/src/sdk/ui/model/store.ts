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

export class Store<TState, TAction extends { type: string } = { type: string } & any> {
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

            this.devTools.subscribe((message: any) => {
                if (message.type === 'DISPATCH' && message.payload.type === 'JUMP_TO_ACTION') {
                    this.setState(JSON.parse(message.state));
                }
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

    public dispatch = (action: TAction | Function) => {
        if (isFunction(action)) {
            action(this);
            return;
        }

        const newState = this.reducer(this.state, action);

        if (this.devTools) {
            this.devTools.send(action, newState);
        }

        this.setState(newState);
    };

    private setState(newState: any) {
        if (newState !== this.state) {
            for (const listener of this.listeners) {
                listener(newState);
            }

            this.state = newState;
        }
    }
}
