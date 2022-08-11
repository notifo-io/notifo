/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Input } from 'reactstrap';
import { loadIntegrations, useApp, useIntegrations } from '@app/state';
import { texts } from '@app/texts';
import { FormEditorOption, FormEditorProps, Forms } from './Forms';

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
        return (
            <Forms.Row {...props}>
                <Input value={texts.common.noIntegration} disabled />
            </Forms.Row>
        );
    }

    return (
        <Forms.Select {...props} options={options} />
    );
};
