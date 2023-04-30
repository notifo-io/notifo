/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import Split from 'react-split';
import { Alert, Input } from 'reactstrap';
import { useEventCallback } from '@app/framework';
import { usePreview } from './helpers';

export interface EmailTextEditorProps {
    // The value.
    value?: string | null;

    // The app name.
    appId: string;

    // When the text has changed.
    onChange: (value: string) => void;

    // Called when the focus has been lost.
    onBlur: () => void;
}

export const EmailTextEditor = (props: EmailTextEditorProps) => {
    const { 
        appId,
        onBlur,
        onChange,
        value,
    } = props;

    const emailMarkup = value || '';
    const emailPreview = usePreview(appId, emailMarkup, 'Text');

    const doChange = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        onChange(event.target.value);
    });

    const error = emailPreview.rendering.errors?.find(x => !x.line || x.line < 0);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <Input type='textarea' value={value || ''} onChange={doChange} onBlur={onBlur} spellCheck={false} />
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
