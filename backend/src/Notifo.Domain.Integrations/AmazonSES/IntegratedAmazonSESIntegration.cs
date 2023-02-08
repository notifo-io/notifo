// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Integrations.Smtp;
using Notifo.Infrastructure;
using Notifo.Infrastructure.KeyValueStore;
using Notifo.Infrastructure.Validation;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.AmazonSES;

public sealed class IntegratedAmazonSESIntegration : IIntegration, IInitializable, IEmailSender
{
    private readonly IKeyValueStore keyValueStore;
    private readonly SmtpIntegration emailSender;
    private readonly AmazonSESOptions emailOptions;
    private AmazonSimpleEmailServiceClient amazonSES;

    public static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", PropertyType.Text)
    {
        Pattern = Patterns.Email,
        EditorLabel = Texts.Email_FromEmailLabel,
        EditorDescription = Texts.Email_FromEmailDescription,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", PropertyType.Text)
    {
        EditorLabel = Texts.Email_FromNameLabel,
        EditorDescription = Texts.Email_FromNameDescription,
        IsRequired = true
    };

    public static readonly IntegrationProperty AdditionalFromEmails = new IntegrationProperty("additionalFromEmails", PropertyType.MultilineText)
    {
        EditorLabel = Texts.Email_AdditionalFromEmailsLabel,
        EditorDescription = Texts.Email_AdditionalFromEmailsDescription,
        IsRequired = false
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "AmazonSES",
            Texts.AmazonSES_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='14.7 23 39.515 24.641' xml:space='preserve' width='39.515' height='24.641'><path style='fill:#f7981f' transform='translate(-1.602 -.429)' d='m28.182 40.34-1.067-4.576h-.025l-1.106 4.576z'/><path d='M14.7 40.315v.666c0 3.311 3.579 6.66 7.991 6.66h23.533c4.412 0 7.991-3.35 7.991-6.66v-.666c0-3.078-3.098-6.943-7.081-7.283a5.118 5.118 0 0 0-8.011-4.055C37.543 25.457 34.016 23 29.907 23c-5.579 0-10.101 4.521-10.101 10.102 0 .1.012.195.015.293-2.993.867-5.121 4.371-5.121 6.92zm12.699 3.055-.572-2.275H24.11l-.599 2.275h-1.547l2.639-9.283h1.898l2.444 9.283zm9.012 0h-1.716l-1.196-6.994h-.026L32.29 43.37h-1.716l-1.795-9.283h1.496l1.221 7.215h.027l1.221-7.215h1.561l1.248 7.254h.026l1.209-7.254h1.469zm5.433.181c-2.301 0-2.821-1.533-2.821-2.834v-.221h1.482v.234c0 1.131.494 1.703 1.521 1.703.936 0 1.403-.664 1.403-1.354 0-.975-.493-1.404-1.325-1.65l-1.015-.352c-1.353-.52-1.937-1.221-1.937-2.547 0-1.691 1.144-2.627 2.886-2.627 2.379 0 2.626 1.482 2.626 2.443v.209h-1.482v-.195c0-.846-.377-1.34-1.3-1.34-.637 0-1.248.352-1.248 1.34 0 .793.403 1.195 1.392 1.572l1 .365c1.313.467 1.886 1.184 1.886 2.457.001 1.979-1.196 2.797-3.068 2.797z' style='fill:#f7981f'/><path d='m24.603 34.087-2.639 9.283h1.547l.599-2.275h2.717l.572 2.275h1.547l-2.444-9.283zm-.221 5.824 1.105-4.576h.025l1.066 4.576z' style='fill:#fff'/><path style='fill:#fff' transform='translate(-1.602 -.429)' d='M35.906 34.516h-1.56l-1.221 7.214h-.027l-1.221-7.214h-1.496l1.795 9.283h1.716l1.182-6.994h.027l1.196 6.994h1.716l1.845-9.283H38.39l-1.209 7.254h-.027z'/><path d='m43.027 38.3-1-.365c-.988-.377-1.392-.779-1.392-1.572 0-.988.611-1.34 1.248-1.34.923 0 1.3.494 1.3 1.34v.195h1.482v-.209c0-.961-.247-2.443-2.626-2.443-1.742 0-2.886.936-2.886 2.627 0 1.326.584 2.027 1.937 2.547l1.015.352c.832.246 1.325.676 1.325 1.65 0 .689-.468 1.354-1.403 1.354-1.027 0-1.521-.572-1.521-1.703v-.234h-1.482v.221c0 1.301.521 2.834 2.821 2.834 1.872 0 3.068-.818 3.068-2.795 0-1.276-.573-1.993-1.886-2.459z' style='fill:#fff'/></svg>",
            new List<IntegrationProperty>
            {
                FromEmailProperty,
                FromNameProperty,
                AdditionalFromEmails
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.AmazonSES_Description
        };

    public IntegratedAmazonSESIntegration(IKeyValueStore keyValueStore, IOptions<AmazonSESOptions> emailOptions, SmtpIntegration emailSender)
    {
        this.emailOptions = emailOptions.Value;
        this.emailSender = emailSender;
        this.keyValueStore = keyValueStore;
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        amazonSES = new AmazonSimpleEmailServiceClient(
            emailOptions.AwsAccessKeyId,
            emailOptions.AwsSecretAccessKey,
            RegionEndpoint.GetBySystemName(emailOptions.Region));

        await amazonSES.GetSendQuotaAsync(ct);
    }

    public Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage request,
        CancellationToken ct)
    {
        FillContext(context);

        return emailSender.SendAsync(context, request, ct);
    }

    private void FillContext(IntegrationContext context)
    {
        context.Properties[SmtpIntegration.HostProperty.Name] = emailOptions.Host;
        context.Properties[SmtpIntegration.HostPortProperty.Name] = emailOptions.HostPort.ToString(CultureInfo.InvariantCulture);
        context.Properties[SmtpIntegration.UsernameProperty.Name] = emailOptions.Username;
        context.Properties[SmtpIntegration.PasswordProperty.Name] = emailOptions.Password;
    }

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        var fromEmails = GetEmailAddresses(context.Properties).ToList();

        if (fromEmails.Count == 0)
        {
            return IntegrationStatus.Verified;
        }

        // Ensure that the email address is not used by another app.
        await ValidateEmailAddressesAsync(context, fromEmails, ct);

        var previousEmails = GetEmailAddresses(previous?.Properties).ToList();

        if (previousEmails.SetEquals(fromEmails, StringComparer.OrdinalIgnoreCase))
        {
            return IntegrationStatus.Verified;
        }

        // Remove unused email addresses to make them available for other apps.
        await CleanEmailsAsync(previousEmails.Except(fromEmails), ct);

        var unconfirmed = await GetUnconfirmedAsync(fromEmails, ct);

        // If all email addresses are already confirmed, we can use the integration.
        if (unconfirmed.Count == 0)
        {
            return IntegrationStatus.Verified;
        }

        foreach (var email in unconfirmed)
        {
            await VerifyAsync(email, default);
        }

        return IntegrationStatus.Pending;
    }

    public async Task OnRemovedAsync(IntegrationContext context,
        CancellationToken ct)
    {
        // Remove unused email addresses to make them available for other apps.
        await CleanEmailsAsync(GetEmailAddresses(context.Properties), ct);
    }

    public async Task<IntegrationStatus> CheckStatusAsync(IntegrationConfiguration configured,
        CancellationToken ct)
    {
        // Check the status every few minutes to update the integration.
        return await GetStatusAsync(GetEmailAddresses(configured.Properties).ToList(), ct);
    }

    private async Task ValidateEmailAddressesAsync(IntegrationContext context, List<string> fromEmails,
        CancellationToken ct)
    {
        if (!emailOptions.BindEmailAddresses)
        {
            return;
        }

        foreach (var email in fromEmails)
        {
            var key = StoreKey(email);

            if (await keyValueStore.SetIfNotExistsAsync(StoreKey(email), context.AppId, ct) != context.AppId)
            {
                var error = string.Format(CultureInfo.InvariantCulture, Texts.AmazonSES_ReservedEmailAddress, email);

                throw new ValidationException(error);
            }
        }
    }

    private async Task CleanEmailsAsync(IEnumerable<string> emails,
        CancellationToken ct)
    {
        foreach (var email in emails)
        {
            await amazonSES.DeleteIdentityAsync(new DeleteIdentityRequest
            {
                Identity = email
            }, ct);

            await keyValueStore.RemvoveAsync(StoreKey(email), ct);
        }
    }

    private async Task VerifyAsync(string email,
        CancellationToken ct)
    {
        await amazonSES.DeleteIdentityAsync(new DeleteIdentityRequest
        {
            Identity = email
        }, ct);

        await amazonSES.VerifyEmailAddressAsync(new VerifyEmailAddressRequest
        {
            EmailAddress = email
        }, ct);
    }

    private async Task<IntegrationStatus> GetStatusAsync(List<string> fromEmails,
        CancellationToken ct)
    {
        var request = new GetIdentityVerificationAttributesRequest
        {
            Identities = fromEmails.ToList()
        };

        var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

        var statuses = new List<IntegrationStatus>();

        foreach (var emailAddress in fromEmails)
        {
            var status = IntegrationStatus.Pending;

            if (response.VerificationAttributes.TryGetValue(emailAddress, out var result))
            {
                status = MapStatus(result.VerificationStatus);
            }

            if (status == IntegrationStatus.VerificationFailed)
            {
                return status;
            }

            statuses.Add(status);
        }

        if (statuses.All(x => x == IntegrationStatus.Verified))
        {
            return IntegrationStatus.Verified;
        }

        return IntegrationStatus.Pending;
    }

    private async Task<List<string>> GetUnconfirmedAsync(List<string> fromEmails,
        CancellationToken ct)
    {
        var request = new GetIdentityVerificationAttributesRequest
        {
            Identities = fromEmails.ToList()
        };

        var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

        var result = new List<string>();

        foreach (var emailAddress in fromEmails)
        {
            if (!response.VerificationAttributes.TryGetValue(emailAddress, out var item) || item.VerificationStatus != VerificationStatus.Success)
            {
                result.Add(emailAddress);
            }
        }

        return result;
    }

    private static IntegrationStatus MapStatus(VerificationStatus status)
    {
        if (status.Equals(VerificationStatus.NotStarted))
        {
            return IntegrationStatus.Pending;
        }

        if (status.Equals(VerificationStatus.Pending))
        {
            return IntegrationStatus.Pending;
        }

        if (status.Equals(VerificationStatus.Success))
        {
            return IntegrationStatus.Verified;
        }

        return IntegrationStatus.VerificationFailed;
    }

    private static IEnumerable<string> GetEmailAddresses(IReadOnlyDictionary<string, string>? properties)
    {
        if (properties == null)
        {
            yield break;
        }

        var hasAdded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var fromEmail = FromEmailProperty.GetString(properties);

        if (!string.IsNullOrWhiteSpace(fromEmail))
        {
            if (hasAdded.Add(fromEmail))
            {
                yield return fromEmail;
            }
        }

        var additionalEmails = AdditionalFromEmails.GetString(properties);

        if (string.IsNullOrWhiteSpace(additionalEmails))
        {
            yield break;
        }

        var emails = additionalEmails.Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var email in emails)
        {
            var trimmed = email.Trim().ToLowerInvariant();

            if (hasAdded.Add(trimmed))
            {
                yield return trimmed;
            }
        }
    }

    private static string StoreKey(string email)
    {
        return $"{nameof(IntegratedAmazonSESIntegration)}_{email}";
    }
}
