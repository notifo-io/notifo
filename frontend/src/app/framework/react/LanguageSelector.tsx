/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, ButtonGroup } from 'reactstrap';

export interface LanguageSelectorProps {
    // The available languages.
    languages: ReadonlyArray<string>;

    // The selected language.
    language?: string;

    // Triggered when the language is selected.
    onSelect?: (language: string) => void;
}

export const LanguageSelector = (props: LanguageSelectorProps) => {
    const { language, languages, onSelect } = props;

    return (
        <ButtonGroup>
            {languages.map(l => (
                <Button key={l} size='sm' color='secondary' outline={language !== l} className='btn-flat' onClick={() => onSelect && onSelect(l)} tabIndex={-1}>
                    {l}
                </Button>
            ))}
        </ButtonGroup>
    );
};
