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

type RequestCode = { code: any };

type MarkupResponse = { rendering: EmailPreviewDto; emailMarkup?: string };

export function usePreview(appId: string, type: EmailPreviewType, kind: string | undefined): [MarkupResponse, string, (value: string) => void] {
    const [emailMarkup, setEmailMarkup] = React.useState<string>('');
    const [emailPreview, setEmailPreview] = React.useState<MarkupResponse>({ rendering: {} });

    const status = React.useRef<RequestCode>({ code: '0' });

    React.useEffect(() => {
        const timeout = setTimeout(async () => {
            const code = new Date().getTime();

            status.current.code = code;

            try {
                const rendering = await Clients.EmailTemplates.postPreview(appId, { template: emailMarkup, type, kind });

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
    }, [appId, emailMarkup, kind, type]);

    return [emailPreview, emailMarkup, setEmailMarkup];
}

export const tags: any = {
    '!top': ['mjml'],
    'mj-accordion': {
        attrs: {
            'border': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'icon-align': null,
            'icon-height': null,
            'icon-position': null,
            'icon-unwrapped-alt': null,
            'icon-unwrapped-url': null,
            'icon-width': null,
            'icon-wrapped-alt': null,
            'icon-wrapped-url': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
        },
    },
    'mj-accordion-element': {
        attrs: {
            'background-color': null,
            'css-class': null,
            'font-family': null,
            'icon-align': null,
            'icon-height': null,
            'icon-position': null,
            'icon-unwrapped-alt': null,
            'icon-unwrapped-url': null,
            'icon-width': null,
            'icon-wrapped-alt': null,
            'icon-wrapped-url': null,
        },
    },
    'mj-accordion-title': {
        attrs: {
            'background-color': null,
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
        },
    },
    'mj-accordion-text': {
        attrs: {
            'background-color': null,
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
        },
    },
    'mj-button': {
        attrs: {
            'align': null,
            'background-color': null,
            'border-bottom': null,
            'border-left': null,
            'border-radius': null,
            'border-right': null,
            'border-top': null,
            'border': null,
            'color': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-style': null,
            'font-weight': null,
            'height': null,
            'href': null,
            'inner-padding': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'rel': null,
            'text-decoration': null,
            'text-transform': null,
            'vertical-align': null,
            'width': null,
        },
    },
    'mj-carousel': {
        attrs: {
            'align': null,
            'background-color': null,
            'border-radius': null,
            'css-class': null,
            'icon-width': null,
            'left-icon': null,
            'right-icon': null,
            'tb-border-radius': null,
            'tb-border': null,
            'tb-hover-border-color': null,
            'tb-selected-border-color': null,
            'tb-width': null,
            'thumbnails': null,
        },
    },
    'mj-carousel-image': {
        attrs: {
            'alt': null,
            'css-class': null,
            'href': null,
            'rel': null,
            'src': null,
            'thumbnails-src': null,
            'title': null,
        },
    },
    'mj-class': {
        attrs: {
            'name': null,
        },
    },
    'mj-column': {
        attrs: {
            'background-color': null,
            'border-bottom': null,
            'border-left': null,
            'border-radius': null,
            'border-right': null,
            'border-top': null,
            'border': null,
            'css-class': null,
            'vertical-align': null,
            'width': null,
        },
    },
    'mj-divider': {
        attrs: {
            'border-color': null,
            'border-style': null,
            'border-width': null,
            'container-background-color': null,
            'css-class': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'width': null,
        },
    },
    'mj-group': {
        attrs: {
            'background-color': null,
            'css-class': null,
            'vertical-align': null,
            'width': null,
        },
    },
    'mj-font': {
        attrs: {
            'css-class': null,
            'href': null,
            'name': null,
        },
    },
    'mj-hero': {
        attrs: {
            'background-color': null,
            'background-height': null,
            'background-position': null,
            'background-url': null,
            'background-width': null,
            'css-class': null,
            'height': null,
            'mode': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'vertical-align': null,
            'width': null,
        },
    },
    'mj-image': {
        attrs: {
            'align': null,
            'alt': null,
            'border-radius': null,
            'border': null,
            'container-background-color': null,
            'css-class': null,
            'height': null,
            'href': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'rel': null,
            'src': null,
            'title': null,
            'width': null,
        },
    },
    'mj-invoice': {
        attrs: {
            'align': null,
            'border': null,
            'color': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'format': null,
            'intl': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
        },
    },
    'mj-invoice-item': {
        attrs: {
            'border': null,
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'line-height': null,
            'name': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'price': null,
            'quantity': null,
            'text-align': null,
        },
    },
    'mj-list': {
        attrs: {
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
        },
    },
    'mj-location': {
        attrs: {
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-weight': null,
            'href': null,
            'img-src': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'rel': null,
        },
    },
    'mj-navbar': {
        attrs: {
            'align': null,
            'css-class': null,
            'hamburger': null,
            'ico-align': null,
            'ico-close': null,
            'ico-color': null,
            'ico-font-family': null,
            'ico-font-size': null,
            'ico-line-height': null,
            'ico-open': null,
            'ico-padding-bottom': null,
            'ico-padding-left': null,
            'ico-padding-right': null,
            'ico-padding-top': null,
            'ico-padding': null,
            'ico-text-decoration': null,
            'ico-text-transform': null,
        },
    },
    'mj-navbar-link': {
        attrs: {
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-style': null,
            'font-weight': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'rel': null,
            'text-decoration': null,
            'text-transform': null,
        },
    },
    'mj-section': {
        attrs: {
            'background-color': null,
            'background-repeat': null,
            'background-size': null,
            'background-url': null,
            'border-bottom': null,
            'border-left': null,
            'border-radius': null,
            'border-right': null,
            'border-top': null,
            'border': null,
            'css-class': null,
            'direction': null,
            'full-width': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'text-align': null,
            'vertical-align': null,
        },
    },
    'mj-social': {
        attrs: {
            'align': null,
            'border-radius': null,
            'color': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-style': null,
            'font-weight': null,
            'icon-size': null,
            'inner-padding': null,
            'line-height': null,
            'mode': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'table-layout': null,
            'vertical-align': null,
        },
    },
    'mj-social-element': {
        attrs: {
            'align': null,
            'background-color': null,
            'border-radius': null,
            'color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-style': null,
            'font-weight': null,
            'href': null,
            'icon-color': null,
            'icon-size': null,
            'line-height': null,
            'name': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'src': null,
            'target': null,
            'text-decoration': null,
        },
    },
    'mj-spacer': {
        attrs: {
            'height': null,
            'css-class': null,
        },
    },
    'mj-table': {
        attrs: {
            'cellpadding': null,
            'cellspacing': null,
            'color': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'table-layout': null,
            'width': null,
        },
    },
    'mj-text': {
        attrs: {
            'align': null,
            'background-color': null,
            'color': null,
            'container-background-color': null,
            'css-class': null,
            'font-family': null,
            'font-size': null,
            'font-style': null,
            'font-weight': null,
            'height': null,
            'letter-spacing': null,
            'line-height': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'text-decoration': null,
            'text-transform': null,
            'vertical-align': null,
        },
    },
    'mj-wrapper': {
        attrs: {
            'background-color': null,
            'background-repeat': null,
            'background-size': null,
            'background-url': null,
            'border-bottom': null,
            'border-left': null,
            'border-radius': null,
            'border-right': null,
            'border-top': null,
            'border': null,
            'css-class': null,
            'full-width': null,
            'padding-bottom': null,
            'padding-left': null,
            'padding-right': null,
            'padding-top': null,
            'padding': null,
            'text-align': null,
            'vertical-align': null,
        },
    },
};

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
