/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable quote-props */

import * as CodeMirror from 'codemirror';
import * as React from 'react';
import { Clients, EmailPreviewDto, EmailPreviewType } from '@app/service';

type MarkupRequest = { code: any };
type MarkupResponse = { rendering: EmailPreviewDto; emailMarkup?: string };

export function usePreview(appId: string, type: EmailPreviewType): [MarkupResponse, string, (value: string) => void] {
    const [emailMarkup, setEmailMarkup] = React.useState<string>('');
    const [emailPreview, setEmailPreview] = React.useState<MarkupResponse>({ rendering: {} });

    const status = React.useRef<MarkupRequest>({ code: '0' });

    React.useEffect(() => {
        const timeout = setTimeout(async () => {
            const code = new Date().getTime();

            status.current.code = code;

            try {
                const rendering = await Clients.EmailTemplates.postPreview(appId, { template: emailMarkup, type });

                if (status.current.code === code) {
                    setEmailPreview({ rendering, emailMarkup });
                }
            } catch (ex: any) {
                if (status.current.code === code) {
                    const rendering: EmailPreviewDto = {
                        errors: [{
                            message: ex.message,
                        }],
                    };

                    setEmailPreview({ rendering, emailMarkup });
                }
            }
        }, 300);

        return () => {
            clearTimeout(timeout);
        };
    }, [appId, emailMarkup, type]);

    return [emailPreview, emailMarkup, setEmailMarkup];
}

export function completeAfter(editor: CodeMirror.Editor, predicate?: (cursor: CodeMirror.Position) => boolean) {
    const cursor = editor.getCursor();

    if (!predicate || predicate(cursor)) {
        setTimeout(() => {
            if (!editor.state.completionActive) {
                const options: any = {
                    completeSingle: false,
                };

                editor.showHint(options);
            }
        }, 100);
    }

    return CodeMirror.Pass;
}

export function completeIfAfterLt(cm: CodeMirror.Editor) {
    return completeAfter(cm, (cursor) => {
        const current = cm.getRange(CodeMirror.Pos(cursor.line, cursor.ch - 1), cursor);

        return current === '<';
    });
}

export function completeIfInTag(cm: CodeMirror.Editor) {
    return completeAfter(cm, (cursor) => {
        const token = cm.getTokenAt(cursor);

        if (token.type === 'string' && (!/['"]/.test(token.string.charAt(token.string.length - 1)) || token.string.length === 1)) {
            return false;
        }

        const inner = CodeMirror.innerMode(cm.getMode(), token.state).state;

        return inner.tagName;
    });
}
