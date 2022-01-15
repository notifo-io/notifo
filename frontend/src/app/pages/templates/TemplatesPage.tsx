/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { loadTemplates, useApp, useTemplates } from '@app/state';
import { TemplateForm } from './TemplateForm';
import { TemplatesList } from './TemplatesList';

export const TemplatesPage = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const [isOpen, setIsOpen] = React.useState(false);
    const templates = useTemplates(x => x.templates);
    const templateCode = useTemplates(x => x.currentTemplateCode);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);

    const template = React.useMemo(() => {
        return templates.items?.find(x => x.code === templateCode);
    }, [templates, templateCode]);

    React.useEffect(() => {
        dispatch(loadTemplates(appId));
    }, [dispatch, appId]);

    const doOpen = React.useCallback(() => {
        setIsOpen(true);
    }, []);

    const doClose = React.useCallback(() => {
        setIsOpen(false);
    }, []);

    return (
        <div className='templates'>
            <TemplatesList onOpen={doOpen} />

            {isOpen &&
                <TemplateForm language={language}
                    onClose={doClose}
                    onLanguageSelect={setLanguage}
                    template={template} />
            }
        </div>
    );
};
