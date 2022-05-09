/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ErrorInfo } from '@app/framework';
import { ConfiguredIntegrationDto, ConfiguredIntegrationsDto, IntegrationDefinitionDto } from '@app/service';

export interface IntegrationsStateInStore {
    integrations: IntegrationsState;
}

export interface IntegrationsState extends Partial<ConfiguredIntegrationsDto> {
    // True if loading integrations.
    loading?: boolean;

    // The loading integrations error.
    loadingError?: ErrorInfo;

    // True if upserting integrations.
    upserting?: boolean;

    // The upserting integrations error.
    upsertingError?: ErrorInfo;
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
        let sortOrder = lhs.definition.title.localeCompare(rhs.definition.title);

        if (sortOrder === 0) {
            sortOrder = priorityKey(rhs.configured) - priorityKey(lhs.configured);
        }

        if (sortOrder === 0) {
            sortOrder = testKey(rhs.configured) - testKey(lhs.configured);
        }

        if (sortOrder === 0) {
            sortOrder = lhs.configuredId.localeCompare(rhs.configuredId);
        }

        return sortOrder;
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
