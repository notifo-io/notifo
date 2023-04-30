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
    value: string;

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
        onBlur,
        onChange,
        schema, 
        value,
    } = props;

    const emailMarkup = value || '';
    const emailPreview = usePreview(appId, emailMarkup, 'Html');

    const error = emailPreview.rendering.errors?.[0];

    return (
        <div className='email-editor'>
            <Split direction='horizontal'>
                <div className='left'>
                    <EmailHtmlTextEditor
                        value={emailMarkup}
                        errors={emailPreview.rendering?.errors}
                        onBlur={onBlur}
                        onChange={onChange}
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
