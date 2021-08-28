/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormEditorOption, FormEditorProps, Forms } from '@app/framework';
import { loadTemplates, useApp, useTemplates } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';

export const TemplateInput = (props: FormEditorProps) => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const templates = useTemplates(x => x.templates);

    React.useEffect(() => {
        if (!templates.isLoaded) {
            dispatch(loadTemplates(appId));
        }
    }, [appId, templates.isLoaded]);

    const options = React.useMemo(() => {
        const result: FormEditorOption<string | undefined>[] = [];

        if (templates.items) {
            for (const { code: label } of templates.items) {
                if (label) {
                    result.push({ label, value: label });
                }
            }
        }

        return result;
    }, [templates.items]);

    return (
        <Forms.Select {...props} options={options} />
    );
};
