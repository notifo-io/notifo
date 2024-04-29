/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ActionReducerMapBuilder, AnyAction, Dispatch } from '@reduxjs/toolkit';
import { ErrorInfo, Types } from './../utils';
import { createApiThunk } from './shared';

export interface ListState<Item, Extra = any> extends Query {
    // The total number of items.
    total: number;

    // Extra information from the search.
    extra?: Extra;

    // The loading error.
    error?: ErrorInfo | null;

    // True, if at least loaded once.
    isLoaded?: boolean;

    // True, if currently loading.
    isLoading?: boolean;

    // True, if currently loading more items.
    isLoadingMore?: boolean;

    // The current items.
    items?: ReadonlyArray<Item>;
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
    sorting?: Sorting | null;
}

export interface SearchRequest extends Query {
    // The number of items to take.
    take: number;

    // The number of items to skip.
    skip: number;

    // True to reset the results.
    reset?: boolean;
}

export interface ListProps<Item, Params, Extra> {
    // The name of the action.
    name: string;

    // The function to make the actual mutation on the server.
    queryFn: (params: Params, request: SearchRequest) => Promise<{ items: ReadonlyArray<Item>; total: number; extra?: Extra }>;
}

type List<Args, Item, Extra> = ((args: Args) => AnyAction) & {
    // Resets the state.
    reset: () => void;

    // Create the initial state.
    createInitial: (pageSize?: number) => ListState<Item, Extra>;
};

type SearchArgs<Params> = ({ query?: Partial<Query>; reset?: boolean } & Params) | undefined;

type ConfigFunction = {
    with: <Item, Params = {}, Extra = any>(props: ListProps<Item, Params, Extra>) => List<SearchArgs<Params>, Item, Extra>;
};

export function createList<State, Root>(key: keyof State, rootKey: keyof Root): ConfigFunction {
    function configure<Item, Params, Extra>(props: ListProps<Item, Params, Extra>) {
        const thunk = createApiThunk(props.name, async (request: SearchRequest & Params) => {
            const {
                page,
                pageSize,
                reset,
                search,
                skip,
                sorting,
                take,
                ...other
            } = request;

            return props.queryFn(other as any, { page, pageSize, reset, search, skip, sorting, take });
        });

        const loadAction = (args: SearchArgs<Params>) => {
            const action = async (dispatch: Dispatch, getState: () => any) => {
                const state = getState()[rootKey][key] as ListState<Item>;

                if (state.isLoading) {
                    return;
                }
                const { query, reset, ...params } = args || {};

                const request: SearchRequest & Params = { ...query || {}, ...params } as any;

                if (!request.hasOwnProperty('search')) {
                    request.search = state.search;
                } else if (hasChanged(request.search, state.search)) {
                    request.page = 0;
                }

                if (!request.hasOwnProperty('sorting')) {
                    request.sorting = state.sorting;
                }

                if (!Types.isNumber(request.page)) {
                    request.page = state.page;
                }

                if (!Types.isNumber(request.pageSize)) {
                    request.pageSize = state.pageSize;
                }

                request.take = request.pageSize;
                request.skip = request.pageSize * request.page;

                dispatch(thunk(request) as any);
            };

            return action;
        };

        (loadAction as any)['initialize'] = (builder: ActionReducerMapBuilder<State>) => {
            builder.addCase(thunk.pending, (state, action) => {
                const list = (state as any)[key] as ListState<Item, Extra>;
                const { page, pageSize, reset, search, sorting } = action.meta.arg;

                list.error = null;
                list.isLoading = true;
                list.isLoadingMore = Types.isArray(list.items);
                list.page = page;
                list.pageSize = pageSize;
                list.search = search;
                list.sorting = sorting;

                if (reset && Types.isArray(list.items)) {
                    list.items = [];
                }
            });

            builder.addCase(thunk.fulfilled, (state, action) => {
                const list = (state as any)[key] as ListState<Item, Extra>;
                const { extra, items, total } = action.payload;

                list.error = null;
                list.extra = extra;
                list.isLoaded = true;
                list.isLoading = false;
                list.isLoadingMore = false;
                list.items = items;

                if (Types.isNumber(total)) {
                    list.total = total;
                }
            });

            builder.addCase(thunk.rejected, (state, action) => {
                const list = (state as any)[key] as ListState<Item, Extra>;

                list.error = action.payload as ErrorInfo;
                list.isLoading = false;
                list.isLoadingMore = false;
            });

            return builder;
        };

        (loadAction as any)['createInitial'] = (pageSize = 20): ListState<Item, Extra> => ({
            page: 0,
            pageSize,
            total: 0,
        });

        return loadAction as any;
    }

    return { with: configure };
}

function hasChanged(lhs: string | undefined | null, rhs: string | undefined | null) {
    if (!lhs && !rhs) {
        return false;
    }

    return lhs !== rhs;
}
