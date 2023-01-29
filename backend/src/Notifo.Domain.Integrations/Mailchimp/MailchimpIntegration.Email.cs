// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Mailchimp;

public sealed partial class MailchimpIntegration : IEmailSender
{
    private sealed class ResponseMessage
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("reject_reason")]
        public string Reason { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResponseStatus Status { get; set; }
    }

    private enum ResponseStatus
    {
        Sent,
        Queued,
        Rejected,
        Invalid
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage message,
        CancellationToken ct)
    {
        var apiKey = ApiKeyProperty.GetString(context.Properties);
        var fromEmail = FromEmailProperty.GetString(context.Properties);
        var fromName = FromNameProperty.GetString(context.Properties);

        using (var httpClient = httpClientFactory.CreateClient("Mailchimp"))
        {
            var body = new
            {
                key = apiKey,
                message = new
                {
                    subject = message.Subject,
                    html = message.BodyHtml,
                    text = message.BodyText,
                    from_email = fromEmail,
                    from_name = fromName,
                    to = new[]
                    {
                        new
                        {
                            email = message.ToEmail, name = message.ToName
                        }
                    }
                }
            };

            var responseMessage = await httpClient.PostAsJsonAsync("messages/send", body, ct);

            responseMessage.EnsureSuccessStatusCode();

            var responses = await responseMessage.Content.ReadFromJsonAsync<ResponseMessage[]>(cancellationToken: ct);

            if (responses is not { Length: 1 })
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Mailchimp_Error, message.FromEmail);

                throw new DomainException(errorMessage);
            }

            var response = responses[0];
            var responseType = response.Status;

            if (responseType != ResponseStatus.Sent)
            {
                var errorMessage =
                    string.Format(CultureInfo.CurrentCulture, Texts.Mailchimp_Error,
                        message.FromEmail,
                        responseType,
                        response.Reason);

                throw new DomainException(errorMessage);
            }
        }

        return DeliveryResult.Sent;
    }
}
