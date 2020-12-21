/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Dispatch } from 'redux';
import { buildError, ErrorDto, Types } from './../utils';

export interface ListState<T, TExtra = any> extends Query {
    // The total number of items.
    total: number;

    // Extra information from the search.
    extra?: TExtra;

    // The loading error.
    error?: ErrorDto;

    // True if currently loading.
    isLoading?: boolean;

    // True if currently loading more items.
    isLoadingMore?: boolean;

    // The current items.
    items?: ReadonlyArray<T>;
}

export interface Sorting {
    // The sorting field.
    field: string;

    // The sorting order.
    order: 'asc' | 'desc';
}

export interface Query {
    // The total page size.
    pageSize: number;

    // The current page.
    page: number;

    // The search string.
    search?: string;

    // The current sort order.
    sorting?: Sorting;
}

export interface SearchRequest extends Query {
    [x: string]: any;
}

export class List<T, TExtra = any> {
    private readonly LOADING_STARTED = `${this.name.toUpperCase()}_LOADING_STARTED`;
    private readonly LOADING_FAILED = `${this.name.toUpperCase()}_LOADING_FAILED`;
    private readonly LOADING_SUCCESS = `${this.name.toUpperCase()}_LOADING_SUCCESS`;

    constructor(
        private readonly prefix: string,
        private readonly name: string,
        private readonly loader: (request: SearchRequest) => Promise<{ items: ReadonlyArray<T>, total: number, extra?: TExtra }>,
    ) {
    }

    public createInitial(pageSize = 20): ListState<T, TExtra> {
        return {
            page: 0,
            pageSize,
            total: 0,
        };
    }

    public load(request: Partial<Query> = {}, params?: any, reset = false) {
        return async (dispatch: Dispatch, getState: () => any) => {
            const state = this.getCurrentState(getState);

            if (state.isLoading) {
                return;
            }

            const newRequest = request || {};

            if (newRequest.search !== state.search) {
                newRequest.page = 0;
            } else if (!Types.isNumber(newRequest.page)) {
                newRequest.page = state.page;
            }

            if (!Types.isNumber(newRequest.pageSize)) {
                newRequest.pageSize = state.pageSize;
            }

            dispatch({ type: this.LOADING_STARTED, ...newRequest, reset });

            try {
                const { items, total, extra } = await this.loader({ ...newRequest, ...params });

                dispatch({ type: this.LOADING_SUCCESS, items, total, extra });
            } catch (err) {
                const error = buildError(err.status, err.message);

                dispatch({ type: this.LOADING_FAILED, error });
            }
        };
    }

    public changeItems(state: any, updater: (items: ReadonlyArray<T>, state: ListState<T>) => ReadonlyArray<T> | undefined) {
        const list = state[this.name];

        if (!list.items) {
            return list;
        }

        const newItems = updater(list.items, list);

        if (newItems === list.items) {
            return list;
        } else {
            const newList = { ...list };

            newList.items = newItems;
            newList.total += (newItems.length - list.items.length);

            return newList;
        }
    }

    public handleAction(state: any, action: { type: string } & any) {
        switch (action.type) {
            case this.LOADING_STARTED: {
                    const newList = { ...state[this.name], ...action };

                    newList.error = null;
                    newList.isLoading = true;
                    newList.isLoadingMore = !!state.items;

                    if (action.reset && Types.isArray(newList.items)) {
                        newList.items = [];
                    }

                    const newState = { ...state, [this.name]: newList };

                    return newState;
                }
            case this.LOADING_SUCCESS: {
                    const newList = { ...state[this.name] };

                    if (Types.isNumber(action.total)) {
                        newList.total = action.total;
                    }

                    newList.error = null;
                    newList.extra = action.extra;
                    newList.isLoading = false;
                    newList.isLoadingMore = false;
                    newList.items = action.items;

                    const newState = { ...state, [this.name]: newList };

                    return newState;
                }
            case this.LOADING_FAILED: {
                    const newList = { ...state[this.name] };

                    newList.error = action.error;
                    newList.isLoading = false;
                    newList.isLoadingMore = false;

                    const newState = { ...state, [this.name]: newList };

                    return newState;
                }
            default:
                return state;
        }

    }

    private getCurrentState(getState: () => any): ListState<T> {
        return getState()[this.prefix][this.name];
    }
}

export function addOrModify<T>(items: ReadonlyArray<T>, item: T, key: keyof T) {
    const found = items.find(x => x[key] === item[key]);

    if (found) {
        return items.map(x => x[key] === item[key] ? item : x);
    } else {
        return [item, ...items];
    }
}

export function remove<T>(items: ReadonlyArray<T>, id: any, key: keyof T) {
    return items.filter(x => x[key] !== id);
}
