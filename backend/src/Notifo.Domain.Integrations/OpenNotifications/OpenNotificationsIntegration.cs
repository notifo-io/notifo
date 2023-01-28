// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsIntegration : IIntegration
{
    private readonly string providerName;
    private readonly ProviderInfoDto providerInfo;
    private readonly IOpenNotificationsClient client;

    public IntegrationDefinition Definition { get; }

    public OpenNotificationsIntegration(string fullName, string providerName, ProviderInfoDto providerInfo, IOpenNotificationsClient client)
    {
        this.client = client;

        var capabilities = new HashSet<string>();

        if (providerInfo.Type == ProviderInfoDtoType.Sms)
        {
            capabilities.Add(Providers.Sms);
        }

        if (providerInfo.Type == ProviderInfoDtoType.Email)
        {
            capabilities.Add(Providers.Email);
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
                    IsRequired = property.Required,
                    MaxLength = property.MaxLength,
                    MinLength = property.MaxLength,
                    MaxValue = property.MaxValue,
                    MinValue = property.MinValue,
                    AllowedValues = property.AllowedValues?.ToArray()
                };
            }).ToList(),
            new List<UserProperty>(),
            capabilities);

        this.providerName = providerName;
        this.providerInfo = providerInfo;
    }

    public bool CanCreate(Type serviceType, IntegrationContext context)
    {
        if (serviceType == typeof(IEmailSender) && providerInfo.Type == ProviderInfoDtoType.Email)
        {
            return true;
        }

        if (serviceType == typeof(ISmsSender) && providerInfo.Type == ProviderInfoDtoType.Sms)
        {
            return true;
        }

        return false;
    }

    public object? Create(Type serviceType, IntegrationContext context, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, context))
        {
            return new OpenNotificationsSender(providerInfo.Type, context, Definition, providerName, client, serviceProvider);
        }

        return null;
    }

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        var dto = new InstallationRequestDto
        {
            Context = context.ToContext(),
            Properties = context.Properties.ToProperties(Definition),
            Provider = providerName,
        };

        await client.Providers.InstallAsync(dto, ct);

        return IntegrationStatus.Verified;
    }

    public async Task OnRemovedAsync(IntegrationContext context,
        CancellationToken ct)
    {
        var dto = new InstallationRequestDto
        {
            Context = context.ToContext(),
            Properties = context.Properties.ToProperties(Definition),
            Provider = providerName,
        };

        await client.Providers.UninstallAsync(dto, ct);
    }
}
