/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as CodeMirror from 'codemirror';
import * as React from 'react';
import { Types } from './../utils';
import 'codemirror/addon/fold/brace-fold';
import 'codemirror/addon/fold/foldcode';
import 'codemirror/addon/fold/foldgutter';
import 'codemirror/addon/fold/indent-fold';
import 'codemirror/mode/htmlmixed/htmlmixed';
import 'codemirror/mode/javascript/javascript';
import { useEventCallback } from './hooks';

export interface CodeProps {
    // The code mode.
    mode?: 'html' | 'json';

    // True, if the height is calculated automatically.
    autoHeight?: boolean;

    // The actual value.
    value?: any;
}

export const Code = (props: CodeProps) => {
    const { autoHeight, mode, value } = props;
    const [editor, setEditor] = React.useState<CodeMirror.Editor>();

    const doInit = useEventCallback((textarea: HTMLTextAreaElement) => {
        if (!textarea) {
            return;
        }
    
        const editor = CodeMirror.fromTextArea(textarea, {
            foldGutter: true,
            gutters: [
                'CodeMirror-linenumbers',
                'CodeMirror-foldgutter',
            ],
            indentUnit: 2,
            indentWithTabs: false,
            lineNumbers: true,
            lineSeparator: undefined,
            lineWrapping: false,
            readOnly: true,
            tabindex: 0,
            tabSize: 2,
        });

        setEditor(editor);
    });

    React.useEffect(() => {
        editor?.setOption('viewportMargin', autoHeight ? Infinity : 10);
    }, [autoHeight, editor]);

    React.useEffect(() => {
        let actualMode: any;

        if (mode === 'html') {
            actualMode = { name: 'htmlmixed' };
        } else {
            actualMode = { name: 'javascript', json: true };
        }

        editor?.setOption('mode', actualMode);
    }, [editor, mode]);

    React.useEffect(() => {
        let actualText: string;

        if (Types.isString(value)) {
            actualText = value;
        } else {
            actualText = JSON.stringify(value || {}, undefined, 2);
        }

        editor?.setValue(actualText);
    }, [editor, value]);

    return (
        <div className={classNames('code-editor', { auto: autoHeight })} style={{ fontSize: '14px' }}>
            <textarea ref={doInit} />
        </div>
    );
};