// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Telekom;

public sealed class TelekomIntegration : IIntegration
{
    private static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Text)
    {
        EditorLabel = Texts.Telekom_ApiKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    private static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
    {
        EditorLabel = Texts.Telekom_PhoneNumberLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Telekom",
            Texts.Telekom_Name,
            "./integrations/telekom.svg",
            new List<IntegrationProperty>
            {
                ApiKeyProperty,
                PhoneNumberProperty
            },
            new List<UserProperty>(),
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.Telekom_Description
        };

    public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
    {
        return serviceType == typeof(ISmsSender);
    }

    public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, id, configured))
        {
            var apikey = ApiKeyProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(apikey))
            {
                return null;
            }

            var phoneNumber = PhoneNumberProperty.GetNumber(configured);

            if (phoneNumber == 0)
            {
                return null;
            }

            return new TelekomSmsSender(
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                apikey,
                phoneNumber.ToString(CultureInfo.InvariantCulture));
        }

        return null;
    }
}
