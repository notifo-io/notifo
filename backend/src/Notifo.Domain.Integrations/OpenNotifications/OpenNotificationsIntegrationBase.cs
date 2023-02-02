// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public abstract class OpenNotificationsIntegrationBase : IIntegration
{
    protected string ProviderName { get; }

    protected ProviderInfoDto ProviderInfo { get; }

    protected IOpenNotificationsClient Client { get; }

    public IntegrationDefinition Definition { get; }

    protected OpenNotificationsIntegrationBase(string fullName, string providerName, ProviderInfoDto providerInfo, IOpenNotificationsClient client)
    {
        var capabilities = new HashSet<string>
        {
            "Open Notifications"
        };

        if (providerInfo.Type == ProviderInfoDtoType.Sms)
        {
            capabilities.Add(Providers.Sms);
        }

        if (providerInfo.Type == ProviderInfoDtoType.Email)
        {
            capabilities.Add(Providers.Email);
        }

        var hasSummaryProperty = providerInfo.Properties.Any(x => x.Value.Summary);

        bool MakeSummary(PropertyInfoDto property)
        {
            if (property.Summary)
            {
                return true;
            }

            if (!hasSummaryProperty && property.Type == PropertyInfoDtoType.String)
            {
                hasSummaryProperty = true;
                return true;
            }

            return false;
        }

        Definition = new IntegrationDefinition(
            fullName,
            providerInfo.DisplayName,
            providerInfo.LogoSvg!,
            providerInfo.Properties.Select(x =>
            {
                var (name, property) = x;

                var type = PropertyType.Text;

                switch (property.Type)
                {
                    case PropertyInfoDtoType.Boolean:
                        type = PropertyType.Boolean;
                        break;
                    case PropertyInfoDtoType.Number:
                        type = PropertyType.Number;
                        break;
                    case PropertyInfoDtoType.Secret:
                        type = PropertyType.Password;
                        break;
                    case PropertyInfoDtoType.String:
                        type = PropertyType.Text;
                        break;
                    case PropertyInfoDtoType.Text:
                        type = PropertyType.MultilineText;
                        break;
                    case PropertyInfoDtoType.Url:
                        type = PropertyType.Text;
                        break;
                }

                return new IntegrationProperty(name, type)
                {
                    AllowedValues = property.AllowedValues?.ToArray(),
                    DefaultValue = property.DefaultValue?.ToString(),
                    EditorDescription = property.Description.Values.FirstOrDefault(),
                    EditorLabel = property.DisplayName.Values.FirstOrDefault(),
                    IsRequired = property.Required,
                    MaxLength = property.MaxLength,
                    MaxValue = property.MaxValue,
                    MinLength = property.MaxLength,
                    MinValue = property.MinValue,
                    Summary = MakeSummary(property)
                };
            }).ToList(),
            new List<IntegrationProperty>(),
            capabilities)
        {
            Description = providerInfo.Description.Values.FirstOrDefault(),
        };

        Client = client;
        ProviderName = providerName;
        ProviderInfo = providerInfo;
    }

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        try
        {
            var dto = new InstallationRequestDto
            {
                Context = context.ToContext(),
                Properties = context.Properties.ToProperties(Definition),
                Provider = ProviderName,
                WebhookUrl = context.WebhookUrl,
            };

            await Client.Providers.InstallAsync(dto, ct);

            return IntegrationStatus.Verified;
        }
        catch (OpenNotificationsException ex)
        {
            throw ex.ToDomainException();
        }
    }

    public async Task OnRemovedAsync(IntegrationContext context,
        CancellationToken ct)
    {
        try
        {
            var dto = new InstallationRequestDto
            {
                Context = context.ToContext(),
                Properties = context.Properties.ToProperties(Definition),
                Provider = ProviderName,
                WebhookUrl = context.WebhookUrl
            };

            await Client.Providers.UninstallAsync(dto, ct);
        }
        catch (OpenNotificationsException ex)
        {
            throw ex.ToDomainException();
        }
    }
}
