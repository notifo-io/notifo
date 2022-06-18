// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure;
using PhoneNumbers;

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed class MessageBirdClient : IMessageBirdClient
    {
        private static readonly char[] TrimChars = { ' ', '+', '0' };
        private readonly MessageBirdOptions options;
        private readonly IHttpClientFactory httpClientFactory;

        public MessageBirdClient(IHttpClientFactory httpClientFactory, IOptions<MessageBirdOptions> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.options = options.Value;
        }

        public async Task<SmsResponse> SendSmsAsync(SmsMessage message,
            CancellationToken ct)
        {
            Guard.NotNull(message);

            var (originator, to, body, reference, reportUrl) = message;

            if (body.Length > 140)
            {
                ThrowHelper.ArgumentException("Text must not have more than 140 characters.", nameof(message));
            }

            to = PhoneNumberUtil.Normalize(to).TrimStart(TrimChars);

            if (!long.TryParse(to, NumberStyles.Integer, CultureInfo.InvariantCulture, out var recipient))
            {
                ThrowHelper.ArgumentException("Not a valid phone number.", nameof(message));
            }

            using (var client = httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AccessKey", options.AccessKey);

                var request = new
                {
                    originator,
                    body,
                    reportUrl,
                    reference,
                    recipients = new[]
                    {
                        recipient
                    }
                };

                var response = await client.PostAsJsonAsync("https://rest.messagebird.com/messages", request, ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SmsResponse>((JsonSerializerOptions?)null, ct);

                    return result!;
                }

                throw await HandleErrorAsync(response, ct);
            }
        }

        public Task<ConversationResponse> SendWhatsAppAsync(WhatsAppTemplateMessage message,
            CancellationToken ct)
        {
            Guard.NotNull(message);

            var (from, to, templateNamespace, templateName, code) = message;

            var request = new
            {
                type = "hsm",
                to,
                from,
                content = new
                {
                    hsm = new Dictionary<string, object>
                    {
                        ["templateName"] = templateName,
                        ["language"] = new
                        {
                            policy = "deterministic",
                            code
                        },
                        ["namespace"] = templateNamespace
                    }
                }
            };

            return SendRequestAsync(request, ct);
        }

        public Task<ConversationResponse> SendWhatsAppAsync(WhatsAppTextMessage message,
            CancellationToken ct)
        {
            Guard.NotNull(message);

            var (from, to, text) = message;

            var request = new
            {
                type = "text",
                to,
                from,
                content = new
                {
                    text
                }
            };

            return SendRequestAsync(request, ct);
        }

        private async Task<ConversationResponse> SendRequestAsync<T>(T request,
            CancellationToken ct)
        {
            using (var client = httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AccessKey", options.AccessKey);

                var response = await client.PostAsJsonAsync("https://conversations.messagebird.com/v1/send", request, ct);
                var result = await response.Content.ReadFromJsonAsync<ConversationResponse>((JsonSerializerOptions?)null, ct);

                if (result!.Error != null)
                {
                    throw new HttpIntegrationException<MessageBirdError>(result.Error.Description, (int)response.StatusCode);
                }

                return result;
            }
        }

        public Task<MessageBirdSmsStatus> ParseStatusAsync(HttpContext httpContext)
        {
            var result = new MessageBirdSmsStatus();

            var query = httpContext.Request.Query;

            if (query.TryGetValue("id", out var id))
            {
                result.Id = id;
            }

            if (query.TryGetValue("recipient", out var recipient))
            {
                result.Recipient = recipient;
            }

            if (query.TryGetValue("reference", out var reference))
            {
                result.Reference = reference;
            }

            if (query.TryGetValue("status", out var statusString) && Enum.TryParse<MessageBirdStatus>(statusString, true, out var status))
            {
                result.Status = status;
            }

            if (query.TryGetValue("statusErrorCode", out var codeString) && int.TryParse(codeString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
            {
                result.StatusErrorCode = code;
            }

            return Task.FromResult(result);
        }

        private static async Task<Exception> HandleErrorAsync(HttpResponseMessage response,
            CancellationToken ct)
        {
            var errors = await response.Content.ReadFromJsonAsync<MessageBirdErrors>((JsonSerializerOptions?)null, ct);
            var error = errors?.Errors?.FirstOrDefault();

            if (error != null)
            {
                var message = $"MessageBird request failed: Code={error.Code}, Description={error.Description}";

                return new HttpIntegrationException<MessageBirdError>(message, (int)response.StatusCode, error);
            }
            else
            {
                var message = "MessageBird request failed with unknown error.";

                return new HttpIntegrationException<MessageBirdError>(message, (int)response.StatusCode);
            }
        }
    }
}
