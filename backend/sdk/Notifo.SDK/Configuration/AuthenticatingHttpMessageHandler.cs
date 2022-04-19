// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Http.Headers;

namespace Notifo.SDK.Configuration
{
    internal sealed class AuthenticatingHttpMessageHandler : DelegatingHandler
    {
        private readonly IAuthenticator authenticator;

        public AuthenticatingHttpMessageHandler(IAuthenticator authenticator)
        {
            InnerHandler = new HttpClientHandler();

            this.authenticator = authenticator;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await authenticator.GetBearerTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authenticator.RemoveTokenAsync(token);
            }

            return response;
        }
    }
}
