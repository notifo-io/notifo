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
            "./integrations/amazon-ses.svg",
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

    public Task SendAsync(IntegrationContext context, EmailMessage request,
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
