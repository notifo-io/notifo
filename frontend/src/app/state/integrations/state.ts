/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorDto } from '@app/framework';
import { ConfiguredIntegrationDto, ConfiguredIntegrationsDto, IntegrationDefinitionDto } from '@app/service';

export interface IntegrationsStateInStore {
    integrations: IntegrationsState;
}

export interface IntegrationsState extends ConfiguredIntegrationsDto {
    // True if loading integrations.
    loading?: boolean;

    // The loading integrations error.
    loadingError?: ErrorDto;

    // True if upserting integrations.
    upserting?: boolean;

    // The upserting integrations error.
    upsertingError?: ErrorDto;
}

export function getSummaryProperties(definition: IntegrationDefinitionDto, configured: ConfiguredIntegrationDto) {
    return definition.properties.filter(p => !!p.summary)
        .map(p => ({ label: p.editorLabel || p.name, value: configured.properties[p.name] }));
}

export function getSortedIntegrations(definitions: { [key: string]: IntegrationDefinitionDto }, all: { [key: string]: ConfiguredIntegrationDto }) {
    const result =
        Object.keys(all).map(configuredId => {
            const configured = all[configuredId];

            return {
                configured,
                configuredId,
                definition: definitions[configured.type],
            };
        }).filter(x => !!x.definition);

    result.sort((lhs, rhs) => {
        let result = lhs.definition.title.localeCompare(rhs.definition.title);

        if (result === 0) {
            result = priorityKey(rhs.configured) - priorityKey(lhs.configured);
        }

        if (result === 0) {
            result = testKey(rhs.configured) - testKey(lhs.configured);
        }

        if (result === 0) {
            result = lhs.configuredId.localeCompare(rhs.configuredId);
        }

        return result;
    });

    return result;
}

function priorityKey(configured: ConfiguredIntegrationDto) {
    return configured.priority || 0;
}

function testKey(configured: ConfiguredIntegrationDto) {
    if (configured.test === true) {
        return 2;
    } else if (configured.test === false) {
        return 1;
    } else {
        return 0;
    }
}
