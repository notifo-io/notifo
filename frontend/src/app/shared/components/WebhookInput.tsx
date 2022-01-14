/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { FormEditorOption, FormEditorProps, Forms } from '@app/framework';
import { loadIntegrations, useApp, useIntegrations } from '@app/state';

export const WebhookInput = (props: FormEditorProps) => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const integrations = useIntegrations(x => x.configured);

    React.useEffect(() => {
        if (!integrations) {
            dispatch(loadIntegrations({ appId }));
        }
    }, [dispatch, appId, integrations]);

    const options = React.useMemo(() => {
        const result: FormEditorOption<string | undefined>[] = [{
            label: '',
        }];

        if (integrations) {
            for (const integration of Object.values(integrations)) {
                if (integration.type === 'Webhook') {
                    const label = integration.properties['Name'];

                    if (label) {
                        result.push({ label, value: label });
                    }
                }
            }
        }

        return result;
    }, [integrations]);

    if (options.length <= 1) {
        return null;
    }

    return (
        <Forms.Select {...props} options={options} />
    );
};
