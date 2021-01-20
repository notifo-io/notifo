/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import { IFrame } from '@app/framework';
import * as React from 'react';
import Split from 'react-split';
import { EmailHtmlTextEditor } from './EmailHtmlTextEditor';
import { usePreview } from './helpers';

type OnChange = (value: string) => void;

export interface EmailHtmlEditorProps {
    // The value.
    value: string;

    // The app name.
    appName: string;

    // When the html has changed.
    onChange?: OnChange;

    // Called when the focus has been lost.
    onBlur?: () => void;
}

export const EmailHtmlEditor = (props: EmailHtmlEditorProps) => {
    const { appName, onChange, value } = props;

    const [emailPreview, markup, setMarkup] = usePreview(appName, 'html');

    React.useEffect(() => {
        setMarkup(value);
    }, [value]);

    React.useEffect(() => {
        onChange && onChange(emailPreview.markup!);
    }, [emailPreview]);

    return (
        <div className='email-editor'>
            <Split direction='horizontal'>
                <div className='left'>
                    <EmailHtmlTextEditor value={markup} errors={emailPreview.errors} onChange={setMarkup} />
                </div>

                <div className='right'>
                    <IFrame html={emailPreview?.result} />
                </div>
            </Split>
        </div>
    );
};
