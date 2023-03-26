/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import * as React from 'react';
import Split from 'react-split';
import { Alert } from 'reactstrap';
import { IFrame } from '@app/framework';
import { MjmlSchema } from '@app/service';
import { EmailHtmlTextEditor } from './EmailHtmlTextEditor';
import { usePreview } from './helpers';

export interface EmailHtmlEditorProps {
    // The value.
    initialValue: string;

    // The app name.
    appId: string;

    // The schema.
    schema?: MjmlSchema;

    // When the html has changed.
    onChange: (value: string) => void;

    // Called when the focus has been lost.
    onBlur: () => void;
}

export const EmailHtmlEditor = (props: EmailHtmlEditorProps) => {
    const {
        appId, 
        initialValue,
        onBlur,
        onChange,
        schema,
    } = props;

    const [emailPreview, markup, setMarkup] = usePreview(appId, 'Html');

    React.useEffect(() => {
        onChange(markup);
    }, [markup, onChange]);

    React.useEffect(() => {
        setMarkup(initialValue);
    }, [setMarkup, initialValue]);

    const error = emailPreview.rendering.errors?.[0];

    return (
        <div className='email-editor'>
            <Split direction='horizontal'>
                <div className='left'>
                    <EmailHtmlTextEditor
                        value={markup}
                        errors={emailPreview.rendering?.errors}
                        onBlur={onBlur}
                        onChange={setMarkup}
                        schema={schema}
                    />
                </div>

                <div className='right'>
                    <IFrame html={emailPreview?.rendering?.result} />

                    {error &&
                        <Alert color='danger'>{error.message}</Alert>
                    }
                </div>
            </Split>
        </div>
    );
};
