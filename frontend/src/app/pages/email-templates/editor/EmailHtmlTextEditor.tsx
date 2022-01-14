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
import { EmailFormattingError } from '@app/service';
import { completeAfter, completeIfAfterLt, completeIfInTag, tags } from './helpers';

type OnChange = (value: string) => void;
type OnBlur = () => void;

export interface EmailHtmlTextEditorProps {
    // The value.
    value: string;

    // The template errors.
    errors?: EmailFormattingError[];

    // When the html has changed.
    onChange?: OnChange;

    // Called when the focus has been lost.
    onBlur?: OnBlur;
}

export class EmailHtmlTextEditor extends React.Component<EmailHtmlTextEditorProps> {
    private editor!: CodeMirror.Editor;
    private editorRef = React.createRef<HTMLTextAreaElement>();
    private value = '';

    public componentDidUpdate(prevProps: EmailHtmlTextEditorProps) {
        if (this.props.value !== this.value) {
            this.updateValue();
        }

        if (this.props.errors !== prevProps.errors) {
            this.updateErrors();
        }
    }

    public componentDidMount() {
        const hintOptions: any = {
            schemaInfo: tags,
        };

        if (!this.editorRef.current) {
            return;
        }

        this.editor = CodeMirror.fromTextArea(this.editorRef.current, {
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
            hintOptions,
            indentWithTabs: false,
            indentUnit: 2,
            lineNumbers: true,
            lineSeparator: undefined,
            matchTags: true,
            theme: 'material',
            tabSize: 2,
        });

        this.editor.on('change', () => {
            const { onChange } = this.props;

            if (onChange) {
                const value = this.editor.getValue();

                if (value !== this.value) {
                    this.value = value;

                    onChange(value);
                }
            }
        });

        this.editor.on('blur', () => {
            const { onBlur } = this.props;

            onBlur && onBlur();
        });
    }

    private updateValue() {
        if (this.editor) {
            const { value } = this.props;

            this.editor.setValue(value);
        }
    }

    private updateErrors() {
        if (this.editor) {
            const { errors } = this.props;

            this.editor.setOption('lint', {
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
        }
    }

    public render() {
        return (
            <textarea ref={this.editorRef} />
        );
    }
}
