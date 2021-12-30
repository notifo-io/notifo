/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, ButtonGroup, DropdownItem, DropdownMenu, DropdownToggle, UncontrolledButtonDropdown } from 'reactstrap';

export interface LanguageSelectorProps {
    // The color.
    color?: 'primary' | 'secondary' | 'danger' | 'warning' | 'info';

    // The size.
    size?: 'lg' | 'sm' | 'md';

    // The available languages.
    languages: ReadonlyArray<string>;

    // The selected language.
    language?: string;

    // Triggered when the language is selected.
    onSelect?: (language: string) => void;
}

export const LanguageSelector = (props: LanguageSelectorProps) => {
    const {
        language,
        languages,
        onSelect,
        size,
    } = props;

    const color = props.color || 'secondary';

    return languages.length > 4 ? (
        <UncontrolledButtonDropdown>
            <DropdownToggle color={color} size={size || 'sm'} outline caret>
                {language}
            </DropdownToggle>
            <DropdownMenu right>
                {languages.map(l => (
                    <DropdownItem key={l} onClick={() => onSelect && onSelect(l)}>
                        {l}
                    </DropdownItem>
                ))}
            </DropdownMenu>
        </UncontrolledButtonDropdown>
    ) : (
        <ButtonGroup color={color} size={size || 'sm'}>
            {languages.map(l => (
                <Button key={l} color={color} outline={language !== l} className='btn-flat' onClick={() => onSelect && onSelect(l)} tabIndex={-1}>
                    {l}
                </Button>
            ))}
        </ButtonGroup>
    );
};
