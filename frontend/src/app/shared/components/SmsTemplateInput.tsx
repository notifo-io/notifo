/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormEditorOption, FormEditorProps, Forms } from '@app/framework';
import { getApp, loadSmsTemplates, useApps, useSmsTemplates } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';

export const SmsTemplateInput = (props: FormEditorProps) => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const templates = useSmsTemplates(x => x.templates);

    React.useEffect(() => {
        if (!templates.isLoaded) {
            dispatch(loadSmsTemplates(appId));
        }
    }, [appId, templates.isLoaded]);

    const options = React.useMemo(() => {
        const result: FormEditorOption<string | undefined>[] = [{
            label: '',
        }];

        if (templates.items) {
            for (const { name: label } of templates.items) {
                if (label) {
                    result.push({ label, value: label });
                }
            }
        }

        return result;
    }, [templates.items]);

    if (options.length <= 1) {
        return null;
    }

    return (
        <Forms.Select {...props} options={options} />
    );
};
