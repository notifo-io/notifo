﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure.Validation;
using Twilio.Rest.Lookups.V1;
using Twilio.Types;

namespace Notifo.Domain.Integrations.Twilio;

public sealed class TwilioIntegration : IIntegration
{
    private readonly TwilioClientPool clientPool;

    private static readonly IntegrationProperty AccountSidProperty = new IntegrationProperty("accountSid", PropertyType.Text)
    {
        EditorLabel = Texts.Twilio_AccountSidLabel,
        EditorDescription = null,
        IsRequired = true
    };

    private static readonly IntegrationProperty AuthTokenProperty = new IntegrationProperty("authToken", PropertyType.Text)
    {
        EditorLabel = Texts.Twilio_AuthTokenLabel,
        EditorDescription = null,
        IsRequired = true
    };

    private static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
    {
        EditorLabel = Texts.Twilio_PhoneNumberLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Twilio",
            Texts.Twilio_Name,
            "./integrations/twilio.svg",
            new List<IntegrationProperty>
            {
                AccountSidProperty,
                AuthTokenProperty,
                PhoneNumberProperty
            },
            new List<UserProperty>(),
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.Twilio_Description
        };

    public TwilioIntegration(TwilioClientPool clientPool)
    {
        this.clientPool = clientPool;
    }

    public bool CanCreate(Type serviceType, string id, IntegrationConfiguration configured)
    {
        return serviceType == typeof(ISmsSender);
    }

    public object? Create(Type serviceType, string id, IntegrationConfiguration configured, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, id, configured))
        {
            var accountSid = AccountSidProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(accountSid))
            {
                return null;
            }

            var authToken = AuthTokenProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(authToken))
            {
                return null;
            }

            var phoneNumber = PhoneNumberProperty.GetNumber(configured);

            if (phoneNumber == 0)
            {
                return null;
            }

            var client = clientPool.GetServer(accountSid, authToken);

            return new TwilioSmsSender(
                serviceProvider.GetRequiredService<ISmsCallback>(),
                client,
                phoneNumber.ToString(CultureInfo.InvariantCulture));
        }

        return null;
    }

    public async Task OnConfiguredAsync(AppContext app, string id, IntegrationConfiguration configured, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        var accountSid = AccountSidProperty.GetString(configured);

        if (string.IsNullOrWhiteSpace(accountSid))
        {
            return;
        }

        var authToken = AuthTokenProperty.GetString(configured);

        if (string.IsNullOrWhiteSpace(authToken))
        {
            return;
        }

        var phoneNumber = PhoneNumberProperty.GetNumber(configured);

        if (phoneNumber == 0)
        {
            return;
        }

        try
        {
            var client = clientPool.GetServer(accountSid, authToken);

            await PhoneNumberResource.FetchAsync(ConvertPhoneNumber(phoneNumber), client: client);
        }
        catch
        {
            throw new ValidationException(Texts.Twilio_ErrorInvalidConfig);
        }
    }

    private static PhoneNumber ConvertPhoneNumber(long number)
    {
        return new PhoneNumber($"+{number}");
    }
}
