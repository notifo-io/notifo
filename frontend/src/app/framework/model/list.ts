/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ActionReducerMapBuilder, createAsyncThunk } from '@reduxjs/toolkit';
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

    // The number of items to take.
    take: number;

    // The number of items to skip.
    skip: number;
}

type ListLoader<TItem, TExtra> = (request: SearchRequest) => Promise<{ items: ReadonlyArray<TItem>, total: number, extra?: TExtra }>;
type ListArgs = { query?: Partial<Query>, reset?: boolean } & { [x: string]: any };

export function listThunk<T, TItem, TExtra = any>(prefix: string, key: string, loader: ListLoader<TItem, TExtra>) {
    const action = createAsyncThunk(`${prefix}/${key}/load`, async (args: ListArgs, thunkApi) => {
        const { query, ...params } = args || {};

        const mergedQuery = mergeQuery(query, thunkApi.getState()[prefix][key]);

        try {
            const loaderRequest: SearchRequest = {
                ...mergedQuery,
                take: mergedQuery.pageSize,
                skip: mergedQuery.pageSize * mergedQuery.page,
                ...params,
            };

            const { items, total, extra } = await loader(loaderRequest);

            return { items, total, extra };
        } catch (err) {
            const error = buildError(err.status, err.message);

            return thunkApi.rejectWithValue(error);
        }
    }, {
        condition: (_, thunkApi) => {
            const state = thunkApi.getState()[prefix][key] as ListState<TItem>;

            return !state.isLoading;
        },
    });

    const initialize = function (builder: ActionReducerMapBuilder<T>) {
        builder.addCase(action.pending, (state, action) => {
            const list = state[key] as ListState<TItem>;

            const newQuery = mergeQuery(action.meta.arg.query, list);

            list.error = null;
            list.isLoading = true;
            list.isLoadingMore = !!list.items;
            list.page = newQuery.page;
            list.pageSize = newQuery.pageSize;
            list.search = newQuery.search;
            list.sorting = newQuery.sorting;

            if (action.meta.arg.reset && Types.isArray(list.items)) {
                list.items = [];
            }
        });

        builder.addCase(action.fulfilled, (state, action) => {
            const list = state[key] as ListState<TItem>;

            if (Types.isNumber(action.payload.total)) {
                list.total = action.payload.total;
            }

            list.error = null;
            list.extra = action.payload.extra;
            list.isLoading = false;
            list.isLoadingMore = false;
            list.items = action.payload.items;
        });

        builder.addCase(action.fulfilled, (state, action) => {
            const list = state[key] as ListState<TItem>;

            if (Types.isNumber(action.payload.total)) {
                list.total = action.payload.total;
            }

            list.error = null;
            list.extra = action.payload.extra;
            list.isLoading = false;
            list.isLoadingMore = false;
            list.items = action.payload.items;
        });

        builder.addCase(action.rejected, (state, action) => {
            const list = state['name'] as ListState<TItem>;

            list.error = action.payload as ErrorDto;
            list.isLoading = false;
            list.isLoadingMore = false;
        });

        return builder;
    };

    function mergeQuery(newQuery: Partial<Query> = {}, oldQuery: Query): Query {
        const mergedQuery = newQuery || {};

        if (newQuery.search !== oldQuery.search) {
            mergedQuery.page = 0;
        } else if (Types.isNumber(mergedQuery.page)) {
            mergedQuery.page = oldQuery.page;
        }

        if (!Types.isNumber(mergedQuery.pageSize)) {
            mergedQuery.pageSize = oldQuery.pageSize;
        }

        return mergedQuery as any;
    }

    const createInitial = function (pageSize = 20): ListState<TItem, TExtra> {
        return {
            page: 0,
            pageSize,
            total: 0,
        };
    };

    return { action, initialize, createInitial };
}
