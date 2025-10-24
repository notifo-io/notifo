/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as CodeMirror from 'codemirror';
import 'codemirror/addon/fold/brace-fold';
import 'codemirror/addon/fold/foldcode';
import 'codemirror/addon/fold/foldgutter';
import 'codemirror/addon/fold/indent-fold';
import * as React from 'react';
import { Types } from './../utils';
import { useEventCallback } from './hooks';

export interface CodeEditorProps {
    // The actual value.
    value?: any;

    // The class name.
    className?: string;

    // The additional options.
    initialOptions?: CodeMirror.EditorConfiguration;

    // The additional options.
    options?: CodeMirror.EditorConfiguration;

    // Invoked when the value has been changed.
    onChange?: (value: string) => void;

    // Invoked when blurred.
    onBlur?: () => void;
}

export const CodeEditor = (props: CodeEditorProps) => {
    const {
        className,
        initialOptions,
        onBlur,
        onChange,
        options,
        value,
    } = props;

    const [editor, setEditor] = React.useState<CodeMirror.Editor>();
    const onBlurRef = React.useRef(onBlur);
    const onChangeRef = React.useRef(onChange);
    const valueRef = React.useRef('');
    const ignoreNext = React.useRef(false);

    onBlurRef.current = onBlur;
    onChangeRef.current = onChange;

    const doInit = useEventCallback((textarea: HTMLTextAreaElement) => {
        if (!textarea) {
            return;
        }

        const editor = CodeMirror.fromTextArea(textarea, {
            foldGutter: true,
            gutters: [
                'CodeMirror-lint-markers',
                'CodeMirror-linenumbers',
                'CodeMirror-foldgutter',
            ],
            indentUnit: 2,
            indentWithTabs: false,
            lineNumbers: true,
            lineSeparator: undefined,
            lineWrapping: false,
            theme: 'material',
            tabSize: 2,
            ...initialOptions || {},
        });

        editor.on('change', editor => {
            const newValue = editor.getValue();

            if (valueRef.current !== newValue) {
                valueRef.current = newValue;

                ignoreNext.current = true;
                try {
                    onChangeRef.current?.(newValue); 
                } finally {
                    ignoreNext.current = false;
                }
            }
        });

        editor.on('blur', () => {
            onBlurRef.current?.();
        });

        setEditor(editor);
    });

    React.useLayoutEffect(() => {
        if (!editor) {
            return;
        }

        let actualText: string;

        if (Types.isString(value)) {
            actualText = value;
        } else {
            actualText = JSON.stringify(value || {}, undefined, 2);
        }

        if (valueRef.current !== value) {
            valueRef.current = value;
            editor.setValue(actualText);
        }
    }, [editor, value]);

    React.useLayoutEffect(() => {
        if (!editor || !options) {
            return;
        }

        for (const [key, value] of Object.entries(options)) {
            editor.setOption(key as any, value);
        }
    }, [editor, options]);

    return (
        <div className={classNames('code-editor', className)} style={{ fontSize: '14px' }}>
            <textarea ref={doInit} />
        </div>
    );
};