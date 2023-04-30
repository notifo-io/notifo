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

type MarkupRequest = { requestId?: any };
type MarkupResponse = { rendering: EmailPreviewDto; template?: string };

export function usePreview(appId: string, template: string, type: EmailPreviewType): MarkupResponse {
    const [emailPreview, setEmailPreview] = React.useState<MarkupResponse>({ rendering: {} });

    const status = React.useRef<MarkupRequest>({});

    React.useEffect(() => {
        async function render() {
            const requestId = new Date().getTime();

            status.current.requestId = requestId;

            try {
                const rendering = await Clients.EmailTemplates.postPreview(appId, { template, type });

                if (status.current.requestId === requestId) {
                    setEmailPreview({ rendering, template });
                }
            } catch (ex: any) {
                if (status.current.requestId === requestId) {
                    const rendering: EmailPreviewDto = {
                        errors: [{
                            message: ex.message,
                        } as any],
                    };

                    setEmailPreview({ rendering, template });
                }
            }
        }

        if (!status.current.requestId) {
            render();
            return undefined;
        }

        const timeout = setTimeout(async () => {
            await render();
        }, 2000);

        return () => {
            clearTimeout(timeout);
        };
    }, [appId, template, type]);

    return emailPreview;
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
