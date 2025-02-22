﻿// ==========================================================================
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

namespace Notifo.Domain.Integrations.MessageBird.Implementation;

public sealed class MessageBirdClient(IHttpClientFactory httpClientFactory, IOptions<MessageBirdOptions> options) : IMessageBirdClient
{
    private readonly Func<HttpClient> httpClientFactory = () =>
        {
            var httpClient = httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AccessKey", options.Value.AccessKey);

            return httpClient;
        };

    public async Task<ConversationResponse> GetMessageAsync(string id,
        CancellationToken ct)
    {
        using (var httpClient = httpClientFactory())
        {
            var result = await httpClient.GetFromJsonAsync<ConversationResponse>($"https://conversations.messagebird.com/v1/messages/{id}", ct);

            return result ?? throw new HttpIntegrationException<MessageBirdError>("Failed to deserialize response.");
        }
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

        to = PhoneNumberUtil.Normalize(to);
        to = PhoneNumberHelper.Trim(to);

        if (!long.TryParse(to, NumberStyles.Integer, CultureInfo.InvariantCulture, out var recipient))
        {
            ThrowHelper.ArgumentException("Not a valid phone number.", nameof(message));
        }

        using (var httpClient = httpClientFactory())
        {
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

            var response = await httpClient.PostAsJsonAsync("https://rest.messagebird.com/messages", request, ct);

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

        var (from, to, templateNamespace, templateName, language, reportUrl, parameters) = message;

        var code = language.Replace('-', '_');

        var request = new
        {
            type = "hsm",
            to,
            from,
            content = new
            {
                hsm = new Dictionary<string, object?>
                {
                    ["templateName"] = templateName,
                    ["language"] = new
                    {
                        policy = "deterministic",
                        code
                    },
                    ["params"] = parameters?.Select<string, Dictionary<string, object>>(x => new Dictionary<string, object>
                    {
                        ["default"] = x
                    }),
                    ["namespace"] = templateNamespace
                }
            },
            reportUrl
        };

        return SendRequestAsync(request, ct);
    }

    public Task<ConversationResponse> SendWhatsAppAsync(WhatsAppTextMessage message,
        CancellationToken ct)
    {
        Guard.NotNull(message);

        var (from, to, text, reportUrl) = message;

        var request = new
        {
            type = "text",
            to,
            from,
            content = new
            {
                text
            },
            reportUrl
        };

        return SendRequestAsync(request, ct);
    }

    private async Task<ConversationResponse> SendRequestAsync<T>(T request,
        CancellationToken ct)
    {
        using (var httpClient = httpClientFactory())
        {
            var response = await httpClient.PostAsJsonAsync("https://conversations.messagebird.com/v1/send", request, ct);

            var result = await response.Content.ReadFromJsonAsync<ConversationResponse>((JsonSerializerOptions?)null, ct)
                ?? throw new HttpIntegrationException<MessageBirdError>("Failed to deserialize response.");

            var error = result.Errors?.FirstOrDefault() ?? result.Error;

            if (error != null)
            {
                throw new HttpIntegrationException<MessageBirdError>(error.Description, (int)response.StatusCode, error);
            }

            return result!;
        }
    }

    public Task<SmsWebhookRequest> ParseSmsWebhookAsync(HttpContext httpContext)
    {
        var result = new SmsWebhookRequest();

        var query = httpContext.Request.Query;

        if (query.TryGetValue("id", out var id))
        {
            string? value = id;

            if (value != null)
            {
                result.Id = value;
            }
        }

        if (query.TryGetValue("recipient", out var recipient))
        {
            string? value = recipient;

            if (value != null)
            {
                result.Recipient = value;
            }
        }

        if (query.TryGetValue("reference", out var reference))
        {
            string? value = reference;

            if (value != null)
            {
                result.Reference = value;
            }
        }

        if (query.TryGetValue("status", out var status))
        {
            if (Enum.TryParse<MessageBirdStatus>(status, true, out var value))
            {
                result.Status = value;
            }
        }

        if (query.TryGetValue("statusErrorCode", out var code))
        {
            if (int.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                result.StatusErrorCode = value;
            }
        }

        return Task.FromResult(result);
    }

    public async Task<WhatsAppWebhookRequest> ParseWhatsAppWebhookAsync(HttpContext httpContext)
    {
        var result = (await JsonSerializer.DeserializeAsync<WhatsAppWebhookRequest>(httpContext.Request.Body, cancellationToken: httpContext.RequestAborted))!;

        var query = httpContext.Request.Query;

        foreach (var (key, value) in query)
        {
            string? valueString = value;

            if (valueString != null)
            {
                result.Query[key] = valueString;
            }
        }

        return result;
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
