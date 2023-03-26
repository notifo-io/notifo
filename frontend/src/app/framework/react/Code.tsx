/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as CodeMirror from 'codemirror';
import * as React from 'react';
import 'codemirror/addon/fold/foldcode';
import 'codemirror/addon/fold/foldgutter';
import 'codemirror/mode/javascript/javascript';
import 'codemirror/mode/htmlmixed/htmlmixed';

export interface JsonDetailsProps {
    // The object.
    object: any;
}

export const JsonDetails = React.memo((props: JsonDetailsProps) => {
    const { object } = props;

    return (
        <CodeEditor mode={'javascript'} value={JSON.stringify(object, null, 2)} />
    );
});

export interface CodeDetailsProps {
    // The object.
    value: string;

    // The actual mode
    mode?: string;
}

export const CodeDetails = React.memo((props: CodeDetailsProps) => {
    const { mode, value } = props;

    return (
        <CodeEditor mode={mode || 'javascript'} value={value} autoHeight={true} />
    );
});

interface CodeEditorProps {
    // The code mode.
    mode: string;

    // True, if the height is calculated automatically.
    autoHeight?: boolean;

    // The actual value.
    value: string;
}

export const CodeEditor = (props: CodeEditorProps) => {
    const { autoHeight, mode, value } = props;
    const [editor, setEditor] = React.useState<CodeMirror.Editor>();

    const doInit = React.useCallback((textarea: HTMLTextAreaElement) => {
        if (!textarea) {
            return;
        }
    
        const editor = CodeMirror.fromTextArea(textarea, {
            autoCloseTags: true,
            gutters: [
                'CodeMirror-linenumbers',
                'CodeMirror-foldgutter',
            ],
            indentUnit: 2,
            indentWithTabs: false,
            lineNumbers: true,
            lineSeparator: undefined,
            matchTags: true,
            readOnly: true,
            tabindex: 0,
            tabSize: 2,
            theme: 'material',
        });

        setEditor(editor);
    }, []);

    React.useEffect(() => {
        editor?.setOption('viewportMargin', autoHeight ? Infinity : 10);
    }, [autoHeight, editor]);

    React.useEffect(() => {
        editor?.setOption('mode', mode);
    }, [editor, mode]);

    React.useEffect(() => {
        editor?.setValue(value);
    }, [editor, value]);

    return (
        <div className={classNames('code-editor', { auto: autoHeight })} style={{ fontSize: '14px' }}>
            <textarea ref={doInit} />
        </div>
    );
};