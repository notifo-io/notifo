/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormProps, Forms } from '@app/framework';
import { getApp, loadTemplatesAsync, useApps, useTemplates } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';

export const TemplateInput = (props: FormProps) => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const templates = useTemplates(x => x.templates);

    React.useEffect(() => {
        if (!templates.isLoaded) {
            dispatch(loadTemplatesAsync(appId));
        }
    }, [appId, templates.isLoaded]);

    const options = React.useMemo(() => {
        return templates.items?.map(x => ({ value: x.code!, label: x.code! })) || [];
    }, [templates.items]);

    return (
        <Forms.Select {...props} options={options} />
    );
};
