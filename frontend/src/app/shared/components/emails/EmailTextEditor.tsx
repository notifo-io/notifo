/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import * as React from 'react';
import Split from 'react-split';
import { Input } from 'reactstrap';
import { usePreview } from './helpers';

export interface EmailTextEditorProps {
    // The value.
    value: string;

    // The app name.
    appName: string;

    // When the text has changed.
    onChange?: (value: string) => void;

    // Called when the focus has been lost.
    onBlur?: () => void;
}

export const EmailTextEditor = (props: EmailTextEditorProps) => {
    const { appName, onBlur, onChange, value } = props;

    const [emailPreview, markup, setMarkup] = usePreview(appName, 'text');

    React.useEffect(() => {
        setMarkup(value);
    }, [value]);

    React.useEffect(() => {
        onChange && onChange(emailPreview.markup);
    }, [emailPreview]);

    const doChange = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setMarkup(event.target.value);
    }, []);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <Input type='textarea' value={markup} onChange={doChange} onBlur={onBlur} spellCheck={false} />
                </div>

                <div className='right'>
                    <textarea className='form-control' readOnly value={emailPreview.result}></textarea>
                </div>
            </Split>
        </div>
    );
};
