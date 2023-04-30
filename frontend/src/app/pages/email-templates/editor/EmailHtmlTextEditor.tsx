/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

// tslint:disable: quotemark

import * as CodeMirror from 'codemirror';
import 'codemirror/addon/dialog/dialog';
import 'codemirror/addon/edit/closetag';
import 'codemirror/addon/edit/matchtags';
import 'codemirror/addon/fold/foldcode';
import 'codemirror/addon/fold/foldgutter';
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
import * as React from 'react';
import { useEventCallback } from '@app/framework';
import { EmailPreviewErrorDto, MjmlSchema } from '@app/service';
import { completeAfter, completeIfAfterLt, completeIfInTag } from './helpers';

export interface EmailHtmlTextEditorProps {
    // The value.
    value: string;

    // The template errors.
    errors?: EmailPreviewErrorDto[];

    // The email schema.
    schema?: MjmlSchema;

    // When the html has changed.
    onChange?: (value: string) => void;

    // Called when the focus has been lost.
    onBlur?: () => void;
}

export const EmailHtmlTextEditor = (props: EmailHtmlTextEditorProps) => {
    const {
        errors,
        onBlur,
        onChange,
        schema,
        value,
    } = props;

    const [editor, setEditor] = React.useState<CodeMirror.Editor>();
    const onBlurRef = React.useRef(onBlur);
    const onChangeRef = React.useRef(onChange);
    const valueRef = React.useRef('');

    onBlurRef.current = onBlur;
    onChangeRef.current = onChange;

    const doInit = useEventCallback((textarea: HTMLTextAreaElement) => {
        if (!textarea) {
            return;
        }
    
        const editor = CodeMirror.fromTextArea(textarea, {
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
            gutters: [
                'CodeMirror-lint-markers',
                'CodeMirror-linenumbers',
                'CodeMirror-foldgutter',
            ],
            indentWithTabs: false,
            indentUnit: 2,
            lineNumbers: true,
            lineSeparator: undefined,
            matchTags: true,
            theme: 'material',
            tabSize: 2,
        });

        editor.on('change', () => {
            const currentOnChange = onChangeRef.current;

            if (!currentOnChange) {
                return;
            }

            const value = editor.getValue();

            if (value === valueRef.current) {
                return;
            }

            valueRef.current = value;

            currentOnChange(value);
        });

        editor.on('blur', () => {
            const currentOnBlur = onBlurRef.current;

            if (!currentOnBlur) {
                return;
            }

            currentOnBlur();
        });

        setEditor(editor);
    });

    React.useEffect(() => {
        editor?.setOption('hintOptions', schema ?  { schemaInfo: schema } : undefined);
    }, [editor, schema]);

    React.useEffect(() => {
        if (!editor || valueRef.current === value) {
            return;
        }

        editor.setValue(value);
    }, [editor, value]);

    React.useEffect(() => {
        if (!editor) {
            return;
        }

        editor.setOption('lint', {
            getAnnotations: () => {
                if (!errors) {
                    return [];
                }

                return errors.map(({ message, line }) => {
                    const from = CodeMirror.Pos(line! - 1, 1);

                    return { message, severity: 'error', from, to: from };
                });
            },
        });
    }, [editor, errors]);

    return (
        <div className='email-editor'>
            <textarea ref={doInit} />
        </div>
    );
};