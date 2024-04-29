/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import Split from 'react-split';
import { Alert } from 'reactstrap';
import { CodeEditor, CodeEditorProps, IFrame } from '@app/framework';
import { MjmlSchema } from '@app/service';
import { useErrors, usePreview } from './helpers';
import { completeAfter, completeIfAfterLt, completeIfInTag } from './helpers';
import 'codemirror/addon/dialog/dialog';
import 'codemirror/addon/edit/closetag';
import 'codemirror/addon/edit/matchtags';
import 'codemirror/addon/hint/show-hint';
import 'codemirror/addon/hint/xml-hint';
import 'codemirror/addon/lint/lint';
import 'codemirror/addon/scroll/annotatescrollbar';
import 'codemirror/addon/search/jump-to-line';
import 'codemirror/addon/search/match-highlighter';
import 'codemirror/addon/search/matchesonscrollbar';
import 'codemirror/addon/search/search';
import 'codemirror/addon/search/searchcursor';
import 'codemirror/addon/selection/active-line';
import 'codemirror/mode/xml/xml';

export interface EmailHtmlEditorProps extends CodeEditorProps {
    // The app name.
    appId: string;

    // The schema.
    schema?: MjmlSchema;
}

export const EmailHtmlEditor = (props: EmailHtmlEditorProps) => {
    const {
        appId,
        schema,
        value,
        ...other
    } = props;

    const emailMarkup = value || '';
    const emailPreview = usePreview(appId, emailMarkup, 'Html');

    const initialOptions = React.useMemo(() => {
        const result: CodeEditorProps['options'] = {
            autoCloseTags: true,
            mode: 'xml',
            extraKeys: {
                Tab: (cm) => {
                    if (cm.getMode().name === 'null') {
                        cm.execCommand('insertTab');
                    } else if (cm.somethingSelected()) {
                        cm.execCommand('indentMore');
                    } else {
                        cm.execCommand('insertSoftTab');
                    }
                },
                '\'<\'': completeAfter,
                '\'/\'': completeIfAfterLt,
                '\' \'': completeIfInTag,
                '\'=\'': completeIfInTag,
                'Ctrl-Space': 'autocomplete',
            },
            matchTags: true,
        };

        return result;
    }, []);

    const { lint, error } = useErrors(emailPreview.rendering);

    const options = React.useMemo(() => {
        const result: CodeEditorProps['options'] = {
            hintOptions: schema ? { schemaInfo: schema } : undefined,
            lint,
        };

        return result;
    }, [lint, schema]);

    return (
        <div className='email-editor white'>
            <Split direction='horizontal'>
                <div className='left'>
                    <CodeEditor initialOptions={initialOptions} options={options} value={emailMarkup}
                        className='email-editor' {...other} />
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
