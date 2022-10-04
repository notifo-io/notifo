/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { InputProps } from 'reactstrap';
import { ListState, Query } from './../model';
import { ClearInput } from './ClearInput';
import { useEventCallback } from './hooks';

export interface ListSearchProps extends Pick<InputProps, Exclude<keyof InputProps, 'list'>> {
    // The bootstrap size.
    bsSize?: 'lg' | 'sm' | undefined;

    // The placeholder
    placeholder?: string;

    // The related list.
    list: ListState<any>;

    // When query changed.
    onSearch: (query: Partial<Query>) => void;
}

export const ListSearch = (props: ListSearchProps) => {
    const {
        list,
        field,
        iconAscending,
        iconDescending,
        onSearch,
        ...other
    } = props;

    const [search, setSearch] = React.useState<string | null>();

    React.useEffect(() => {
        const timer = setTimeout(() => {
            if (hasChanged(list.search, search) && onSearch) {
                onSearch({ search: search });
            }
        }, 3000);

        return () => {
            clearInterval(timer);
        };
    }, [list, search, onSearch]);

    React.useEffect(() => {
        setSearch(list.search);
    }, [list.search]);

    const doChange = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setSearch(event.target.value);
    });

    const doPress = useEventCallback((event: React.KeyboardEvent<HTMLInputElement>) => {
        if (hasChanged(list.search, search) && onSearch && isEnter(event)) {
            onSearch({ search });
        }
    });

    const doClear = useEventCallback(() => {
        if (hasChanged(list.search, undefined) && onSearch) {
            onSearch({ search: undefined });
        }
    });

    return (
        <ClearInput {...other} value={search || ''} disabled={list.isLoading}
            onClear={doClear}
            onChange={doChange}
            onKeyPress={doPress}
        />
    );
};

function isEnter(event: React.KeyboardEvent<HTMLInputElement>) {
    return event.key === 'Enter' || event.keyCode === 13;
}

function hasChanged(lhs: string | undefined | null, rhs: string | undefined | null) {
    if (!lhs && !rhs) {
        return false;
    }

    return lhs !== rhs;
}
