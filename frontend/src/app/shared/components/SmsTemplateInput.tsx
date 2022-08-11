/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Input } from 'reactstrap';
import { loadSmsTemplates, useApp, useSmsTemplates } from '@app/state';
import { texts } from '@app/texts';
import { FormEditorOption, FormEditorProps, Forms } from './Forms';

export const SmsTemplateInput = (props: FormEditorProps) => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const templates = useSmsTemplates(x => x.templates);

    React.useEffect(() => {
        if (!templates.isLoaded) {
            dispatch(loadSmsTemplates(appId));
        }
    }, [dispatch, appId, templates.isLoaded]);

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

    if (!templates.isLoaded) {
        return null;
    }

    if (options.length <= 1) {
        return (
            <Forms.Row {...props}>
                <Input value={texts.common.noTemplate} disabled />
            </Forms.Row>
        );
    }

    return (
        <Forms.Select {...props} options={options} />
    );
};
