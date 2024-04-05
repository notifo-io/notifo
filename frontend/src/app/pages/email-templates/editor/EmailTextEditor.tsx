/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import Split from 'react-split';
import { Alert } from 'reactstrap';
import { CodeEditor, CodeEditorProps } from '@app/framework';
import { usePreview } from './helpers';
import 'codemirror/mode/django/django';

export interface EmailTextEditorProps extends CodeEditorProps {
    // The app name.
    appId: string;
}

const OPTIONS = {
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

    const error = emailPreview.rendering.errors?.find(x => !x.lineNumber || x.lineNumber < 0);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <CodeEditor value={value} {...other}
                        initialOptions={OPTIONS} />
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
