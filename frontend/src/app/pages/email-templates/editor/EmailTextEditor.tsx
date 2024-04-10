/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import Split from 'react-split';
import { Alert } from 'reactstrap';
import { CodeEditor, CodeEditorProps } from '@app/framework';
import { useErrors, usePreview } from './helpers';
import 'codemirror/mode/django/django';

export interface EmailTextEditorProps extends CodeEditorProps {
    // The app name.
    appId: string;
}

const INITIAL_OPTIONS = {
    mode: 'django',
};

export const EmailTextEditor = (props: EmailTextEditorProps) => {
    const {
        appId,
        value,
        ...other
    } = props;

    const emailMarkup = value || '';
    const emailPreview = usePreview(appId, emailMarkup, 'Text');

    const { lint, error } = useErrors(emailPreview.rendering);

    const options = React.useMemo(() => {
        const result: CodeEditorProps['options'] = {
            lint,
        };

        return result;
    }, [lint]);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <CodeEditor initialOptions={INITIAL_OPTIONS} options={options} value={emailMarkup}
                        className='email-editor' {...other} />
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
