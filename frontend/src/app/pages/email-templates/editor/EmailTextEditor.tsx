/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import * as React from 'react';
import Split from 'react-split';
import { Alert, Input } from 'reactstrap';
import { usePreview } from './helpers';

export interface EmailTextEditorProps {
    // The initial value.
    initialValue?: string | null;

    // The app name.
    appId: string;

    // The kind ot the template.
    kind?: string | undefined;

    // When the text has changed.
    onChange?: (value: string) => void;

    // Called when the focus has been lost.
    onBlur?: () => void;
}

export const EmailTextEditor = (props: EmailTextEditorProps) => {
    const { appId, kind, onBlur, onChange, initialValue } = props;

    const [emailPreview, markup, setMarkup] = usePreview(appId, 'Text', kind);

    React.useEffect(() => {
        onChange && emailPreview.emailMarkup && onChange(emailPreview.emailMarkup);
    }, [emailPreview.emailMarkup, onChange]);

    React.useEffect(() => {
        setMarkup(initialValue || '');
    }, [setMarkup, initialValue]);

    const doChange = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setMarkup(event.target.value);
    }, [setMarkup]);

    const error = emailPreview.rendering.errors?.find(x => !x.line || x.line < 0);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <Input type='textarea' value={markup} onChange={doChange} onBlur={onBlur} spellCheck={false} />
                </div>

                <div className='right'>
                    <textarea className='form-control' readOnly value={emailPreview.rendering?.result || ''}></textarea>

                    {error &&
                        <Alert color='danger'>{error.message}</Alert>
                    }
                </div>
            </Split>
        </div>
    );
};
