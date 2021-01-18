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

    const { isLoading, search } = list;

    const [value, setValue] = React.useState('');
    const currentSearch = React.useRef<string>();
    const currentValue = React.useRef<string>();

    React.useEffect(() => {
        const timer = setTimeout(() => {
            if (currentSearch.current !== value && onSearch && !list.isLoading) {
                onSearch({ search: value });
            }
        }, 3000);

        return () => {
            clearInterval(timer);
        };
    }, [list.isLoading, value, onSearch]);

    React.useEffect(() => {
        currentSearch.current = search;
        currentValue.current = search;

        setValue(search);
    }, [search]);

    const doChange = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        currentValue.current = event.target.value;

        setValue(event.target.value);
    }, []);

    const doPress = React.useCallback((event: React.KeyboardEvent<HTMLInputElement>) => {
        if (currentSearch.current !== currentValue.current && onSearch && (event.key === 'Enter' || event.keyCode === 13)) {
            onSearch({ search: currentValue.current });
        }
    }, []);

    return (
        <ClearInput {...other} value={value} onChange={doChange} disabled={isLoading} onKeyPress={doPress} />
    );
};
