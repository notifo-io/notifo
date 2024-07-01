/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Numbers } from '@app/framework';
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
            <RenderSVG svg={definition.logo} />
        );
    }

    return (
        <img src={`${getApiUrl()}/api/integrations/image/${type}`} alt={definition.title} title={definition.title} />
    );
};

const RenderSVG = ({ svg }: { svg: string }) => {
    const html = React.useMemo(() => {
        if (!svg) {
            return null;
        }
        

        const prefix = Numbers.guid();

        const div = document.createElement('div');
        div.innerHTML = svg;
        div.querySelectorAll('*').forEach(element => {
            const id = element.getAttribute('id');

            if (id) {
                element.setAttribute('id', `${prefix}${id}`);
            }

            for (let i = 0; i < element.attributes.length; i++) {
                const attribute = element.attributes[i];

                let valueOld = attribute.value;
                let valueNew = valueOld;

                if (valueOld.startsWith('#') && attribute.name === 'xlink:href') {
                    valueNew = valueOld.replace(/#(.*)/g, `#${prefix}$1`);
                } else {
                    valueNew = valueOld.replace(/url\(#(.*)\)/g, `url(#${prefix}$1)`);
                }

                if (valueNew !== valueOld) {
                    element.setAttribute(attribute.name, valueNew);
                }
            }
        });

        return { __html: div.innerHTML };
    }, [svg]);

    if (!html) {
        return null;
    }

    return (
        <div dangerouslySetInnerHTML={html} />
    );
};