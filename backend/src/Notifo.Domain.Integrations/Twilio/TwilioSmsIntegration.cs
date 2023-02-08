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
            "<svg fill='#F22F46' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 30'><path d='M14.4 11.3c0 1.7-1.4 3.1-3.1 3.1S8.2 13 8.2 11.3s1.4-3.1 3.1-3.1 3.1 1.4 3.1 3.1zm-3.1 4.3c-1.7 0-3.1 1.4-3.1 3.1s1.4 3.1 3.1 3.1 3.1-1.4 3.1-3.1-1.4-3.1-3.1-3.1zM30 15c0 8.3-6.7 15-15 15S0 23.3 0 15 6.7 0 15 0s15 6.7 15 15zm-4 0c0-6.1-4.9-11-11-11S4 8.9 4 15s4.9 11 11 11 11-4.9 11-11zm-7.3.6c-1.7 0-3.1 1.4-3.1 3.1s1.4 3.1 3.1 3.1 3.1-1.4 3.1-3.1-1.4-3.1-3.1-3.1zm0-7.4c-1.7 0-3.1 1.4-3.1 3.1s1.4 3.1 3.1 3.1 3.1-1.4 3.1-3.1-1.4-3.1-3.1-3.1zm51.6-2.3c.1 0 .2.1.3.2v3.2c0 .2-.2.3-.3.3H65c-.2 0-.3-.2-.3-.3V6.2c0-.2.2-.3.3-.3h5.3zm-.1 4.5H60c-.1 0-.3.1-.3.3l-1.3 5-.1.3-1.6-5.3c0-.1-.2-.3-.3-.3h-4c-.1 0-.3.1-.3.3l-1.5 5-.1.3-.1-.3-.6-2.5-.6-2.5c0-.1-.2-.3-.3-.3h-8V6.1c0-.1-.2-.3-.4-.2l-5 1.6c-.2 0-.3.1-.3.3v2.7h-1.3c-.1 0-.3.1-.3.3v3.8c0 .1.1.3.3.3h1.3v4.7c0 3.3 1.8 4.8 5.1 4.8 1.4 0 2.7-.3 3.6-.8v-4c0-.2-.2-.3-.3-.2-.5.2-1 .3-1.4.3-.9 0-1.4-.4-1.4-1.4v-3.4h2.9c.1 0 .3-.1.3-.3v-3.2L47.8 24c0 .1.2.3.3.3h4.2c.1 0 .3-.1.3-.3l1.8-5.6.9 2.9.8 2.7c0 .1.2.3.3.3h4.2c.1 0 .3-.1.3-.3l3.8-12.6V24c0 .1.1.3.3.3h5.1c.1 0 .3-.1.3-.3V10.7c0-.1-.1-.3-.2-.3zm6.7-4.5h-5.1c-.1 0-.3.1-.3.3v17.7c0 .1.1.3.3.3h5.1c.1 0 .3-.1.3-.3V6.1c0-.1-.1-.2-.3-.2zm6.8 0h-5.3c-.1 0-.3.1-.3.3v3.1c0 .1.1.3.3.3h5.3c.1 0 .3-.1.3-.3V6.1c0-.1-.1-.2-.3-.2zm-.1 4.5h-5.1c-.1 0-.3.1-.3.3v13.1c0 .1.1.3.3.3h5.1c.1 0 .3-.1.3-.3V10.7c0-.1-.1-.3-.3-.3zm16.1 6.8c0 3.8-3.2 7.1-7.7 7.1-4.4 0-7.6-3.3-7.6-7.1s3.2-7.1 7.7-7.1c4.4 0 7.6 3.3 7.6 7.1zm-5.4.1c0-1.4-1-2.5-2.2-2.4-1.3 0-2.2 1.1-2.2 2.4s1 2.4 2.2 2.4c1.3 0 2.2-1.1 2.2-2.4z'/></svg>\r\n",
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

    public TwilioSmsIntegration(IMemoryCache cache)
    {
        clientPool = new TwilioClientPool(cache);
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
