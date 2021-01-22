/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ActionReducerMapBuilder, createAction, Dispatch } from '@reduxjs/toolkit';
import { buildError, ErrorDto, Types } from './../utils';

export interface ListState<T, TExtra = any> extends Query {
    // The total number of items.
    total: number;

    // Extra information from the search.
    extra?: TExtra;

    // The loading error.
    error?: ErrorDto | null;

    // True if at least loaded once.
    isLoaded?: boolean;

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
    search?: string | null;

    // The current sort order.
    sorting?: Sorting | null;
}

export interface SearchRequest extends Query {
    [x: string]: any;

    // The number of items to take.
    take: number;

    // The number of items to skip.
    skip: number;
}

type ListLoader<TItem, TExtra> = (request: SearchRequest) => Promise<{ items: ReadonlyArray<TItem>, total: number, extra?: TExtra }>;
type ListArg = { query?: Partial<SearchRequest>, reset?: boolean } & { [x: string]: any };

export function listThunk<T, TItem, TExtra = any>(prefix: string, key: string, loader: ListLoader<TItem, TExtra>) {
    const name = `${prefix}/${key}/load`;

    type ActionPendingType = { reset?: boolean, request: SearchRequest };
    type ActionFulfilledType = { items: readonly TItem[], extra?: TExtra, total: number };
    type ActionRejectedType = { error: ErrorDto };

    const actionPending = createAction<ActionPendingType>(`${name}/pending`);
    const actionFulfilled = createAction<ActionFulfilledType>(`${name}/fulfilled`);
    const actionRejected = createAction<ActionRejectedType>(`${name}/rejected`);

    const action = (arg: ListArg) => {
        return async (dispatch: Dispatch, getState: () => any) => {
            const state = getState()[prefix][key] as ListState<T>;

            if (state.isLoading) {
                return;
            }

            const { query, reset, ...params } = arg || {};

            const request: SearchRequest = { ...query || {}, ...params } as any;

            if (hasChanged(request.search, state.search)) {
                request.page = 0;
            }

            if (!Types.isNumber(request.page)) {
                request.page = state.page;
            }

            if (!Types.isNumber(request.pageSize)) {
                request.pageSize = state.pageSize;
            }

            request.take = request.pageSize;
            request.skip = request.pageSize * request.page;

            dispatch(actionPending({ reset, request }));

            try {
                const result = await loader(request);

                dispatch(actionFulfilled(result));
            } catch (err) {
                const error = buildError(err.status, err.message);

                dispatch(actionRejected({ error }));
            }
        };
    };

    const initialize = (builder: ActionReducerMapBuilder<T>) => {
        builder.addCase(actionPending, (state, action) => {
            const list = state[key] as ListState<TItem>;
            const loaded = Types.isArray(list.items);

            const { request, reset } = action.payload;

            list.error = null;
            list.isLoading = true;
            list.isLoadingMore = loaded;
            list.page = request.page;
            list.pageSize = request.pageSize;
            list.search = request.search;
            list.sorting = request.sorting;

            if (reset && loaded) {
                list.items = [];
            }
        });

        builder.addCase(actionFulfilled, (state, action) => {
            const list = state[key] as ListState<TItem>;

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

        builder.addCase(actionRejected, (state, action) => {
            const list = state[key] as ListState<TItem>;

            const { error } = action.payload;

            list.error = error;
            list.isLoading = false;
            list.isLoadingMore = false;
        });

        return builder;
    };

    const createInitial = (pageSize = 20): ListState<TItem, TExtra> => ({
        page: 0,
        pageSize,
        total: 0,
    });

    return { action, initialize, createInitial };
}

function hasChanged(lhs: string | undefined | null, rhs: string | undefined | null) {
    if (!lhs && !rhs) {
        return false;
    }

    return lhs !== rhs;
}
