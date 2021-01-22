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

export interface ListSearchProps extends Pick<InputProps, Exclude<keyof InputProps, 'list'>> {
    // The bootstrap size.
    bsSize?: 'lg' | 'sm';

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

    const [value, setValue] = React.useState<string | null>();

    const currentValue = React.useRef<string | null | undefined>();

    React.useEffect(() => {
        const timer = setTimeout(() => {
            if (hasChanged(list.search, value) && onSearch) {
                onSearch({ search: value });
            }
        }, 3000);

        return () => {
            clearInterval(timer);
        };
    }, [list, value, onSearch]);

    React.useEffect(() => {
        currentValue.current = list.search;

        setValue(list.search);
    }, [list.search]);

    const doChange = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        currentValue.current = event.target.value;

        setValue(event.target.value);
    }, []);

    const doPress = React.useCallback((event: React.KeyboardEvent<HTMLInputElement>) => {
        if (hasChanged(list.search, currentValue.current) && onSearch && isEnter(event)) {
            onSearch({ search: currentValue.current });
        }
    }, [list]);

    const doClear = React.useCallback(() => {
        if (hasChanged(list.search, undefined) && onSearch) {
            onSearch({ search: undefined });
        }
    }, [list]);

    return (
        <ClearInput {...other} value={value || ''} disabled={list.isLoading}
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
