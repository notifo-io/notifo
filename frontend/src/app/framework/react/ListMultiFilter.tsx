/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { Button, Col, Row } from 'reactstrap';
import { useEventCallback } from './hooks';

type Option = { value: string; label: string };

export interface ListMultiFilterProps {
    // The allowed options.
    options: ReadonlyArray<Option>;

    // The values.
    value: string[];

    // Triggered when changed.
    onChange: (value: string[]) => void;
}

export const ListMultiFilter = (props: ListMultiFilterProps) => {
    const { onChange, options, value } = props;

    const doToggle = useEventCallback((toggled: string) => {
        let newValue: string[];

        if (value.indexOf(toggled) >= 0) {
            newValue = value.filter(x => x !== toggled);
        } else {
            newValue = [...value, toggled];
        }

        newValue = newValue.length >= options.length ? [] : newValue;

        onChange(newValue);
    });

    return (
        <Row className='multi-filter' noGutters>
            {options.map(option => 
                <Col key={option.value} style={{ width: `100/${options.length}%` }}>
                    <Button block color='blank' className={classNames('btn-flat', { active: value.indexOf(option.value) >= 0 })} onClick={() => doToggle(option.value)}>
                        {option.label}
                    </Button>
                </Col>,
            )}
        </Row>
    );
};