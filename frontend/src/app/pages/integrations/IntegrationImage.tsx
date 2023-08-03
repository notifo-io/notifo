/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { getApiUrl, IntegrationDefinitionDto } from '@app/service';

export interface IntegrationImageProps {
    // The provider type.
    type: string;

    // The definition.
    definition: IntegrationDefinitionDto;
}

export const IntegrationImage = (props: IntegrationImageProps) => {
    const { definition, type } = props;

    if (definition.logo) {
        return (
            <div dangerouslySetInnerHTML={{ __html: definition.logo }}></div>
        );
    }

    return (
        <img src={`${getApiUrl()}/api/integrations/image/${type}`} alt={definition.title} title={definition.title} />
    );
};
