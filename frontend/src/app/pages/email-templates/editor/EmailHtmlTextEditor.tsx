/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import * as CodeMirror from 'codemirror';
import * as React from 'react';
import { CodeEditor, CodeEditorProps } from '@app/framework';
import { EmailPreviewErrorDto, MjmlSchema } from '@app/service';
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

export interface EmailHtmlTextEditorProps extends CodeEditorProps {
    // The template errors.
    errors?: EmailPreviewErrorDto[];

    // The email schema.
    schema?: MjmlSchema;
}

export const EmailHtmlTextEditor = (props: EmailHtmlTextEditorProps) => {
    const { errors, schema, ...other } = props;

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

    const options = React.useMemo(() => {
        const result: CodeEditorProps['options'] = {
            hintOptions: schema ? { schemaInfo: schema } : undefined,
            lint: {
                getAnnotations: () => {
                    if (!errors) {
                        return [];
                    }
    
                    return errors.map(({ message, lineNumber }) => {
                        const from = CodeMirror.Pos(lineNumber! - 1, 1);
    
                        return { message, severity: 'error', from, to: from };
                    });
                },
            },
        };

        return result;
    }, [errors, schema]);

    return (
        <CodeEditor initialOptions={initialOptions} options={options} className='email-editor' {...other} />
    );
};