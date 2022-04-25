// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Users;

namespace Notifo.Domain.Integrations.Threema
{
    public sealed class ThreemaSimpleMessagingSender : IMessagingSender
    {
        private const string ThreemaPhoneNumber = nameof(ThreemaPhoneNumber);
        private const string ThreemaEmail = nameof(ThreemaEmail);
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string apiIdentity;
        private readonly string apiSecret;

        public ThreemaSimpleMessagingSender(IHttpClientFactory httpClientFactory, string apiIdentity, string apiSecret)
        {
            this.httpClientFactory = httpClientFactory;
            this.apiIdentity = apiIdentity;
            this.apiSecret = apiSecret;
        }

        public bool HasTarget(User user)
        {
            return !string.IsNullOrWhiteSpace(user.EmailAddress) || !string.IsNullOrWhiteSpace(user.PhoneNumber);
        }

        public Task AddTargetsAsync(MessagingJob job, User user)
        {
            var phoneNumber = user.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                job.Targets[ThreemaPhoneNumber] = phoneNumber;
            }

            var email = user.EmailAddress;

            if (!string.IsNullOrWhiteSpace(email))
            {
                job.Targets[ThreemaEmail] = email;
            }

            return Task.CompletedTask;
        }

        public async Task<bool> SendAsync(MessagingJob job, string text,
            CancellationToken ct)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                Exception? exception = null;

                if (job.Targets.TryGetValue(ThreemaPhoneNumber, out var phoneNumber))
                {
                    try
                    {
                        if (await SendAsync(httpClient, "phone", phoneNumber, text, ct))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                if (job.Targets.TryGetValue(ThreemaEmail, out var email))
                {
                    try
                    {
                        if (await SendAsync(httpClient, "email", email, text, ct))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

            return false;
        }

        private async Task<bool> SendAsync(HttpClient httpClient, string toKey, string toValue, string text,
            CancellationToken ct)
        {
            // Read the API documentation: https://gateway.threema.ch/de/developer/api
            const string Url = "https://msgapi.threema.ch/send_simple";

            var parameters = new[]
            {
                new KeyValuePair<string, string>("secret", apiSecret),
                new KeyValuePair<string, string>("from", apiIdentity),
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>(toKey, toValue)
            };

            var form = new FormUrlEncodedContent(parameters!);

            var response = await httpClient.PostAsync(Url, form, ct);

            // BadRequest (400) is returned when the to parameter is invalid.
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }

        public Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}
