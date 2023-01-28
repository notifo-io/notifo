// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure.Validation;
using Twilio.Rest.Lookups.V1;
using Twilio.Types;

namespace Notifo.Domain.Integrations.Twilio;

public sealed partial class TwilioSmsIntegration : IIntegration
{
    private readonly ISmsCallback callback;
    private readonly TwilioClientPool clientPool;

    public static readonly IntegrationProperty AccountSidProperty = new IntegrationProperty("accountSid", PropertyType.Text)
    {
        EditorLabel = Texts.Twilio_AccountSidLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty AuthTokenProperty = new IntegrationProperty("authToken", PropertyType.Text)
    {
        EditorLabel = Texts.Twilio_AuthTokenLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
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
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.Twilio_Description
        };

    public TwilioSmsIntegration(IMemoryCache cache, ISmsCallback callback)
    {
        clientPool = new TwilioClientPool(cache);

        this.callback = callback;
    }

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        var accountSid = AccountSidProperty.GetString(context.Properties);
        var accountToken = AuthTokenProperty.GetString(context.Properties);
        var phoneNumber = PhoneNumberProperty.GetNumber(context.Properties);
        try
        {
            var client = clientPool.GetServer(accountSid, accountToken);

            await PhoneNumberResource.FetchAsync(ConvertPhoneNumber(phoneNumber), client: client);
        }
        catch
        {
            throw new ValidationException(Texts.Twilio_ErrorInvalidConfig);
        }

        return IntegrationStatus.Verified;
    }

    private static PhoneNumber ConvertPhoneNumber(long number)
    {
        return new PhoneNumber($"+{number}");
    }
}
