/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { getApp, loadTemplatesAsync, useApps, useTemplates } from '@app/state';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { TemplateForm } from './TemplateForm';
import { TemplatesList } from './TemplatesList';

export const TemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApps(getApp);
    const appId = app.id;
    const appLanguages = app.languages;
    const templates = useTemplates(x => x.templates);
    const templateCode = useTemplates(x => x.currentTemplateCode);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);

    const template = React.useMemo(() => {
        return templates.items?.find(x => x.code === templateCode);
    }, [templates, templateCode]);

    React.useEffect(() => {
        dispatch(loadTemplatesAsync(appId));
    }, [appId]);

    return (
        <div className='templates'>
            <TemplatesList />

            <TemplateForm language={language} onLanguageSelect={setLanguage} template={template} />
        </div>
    );
};
