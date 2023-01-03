// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Http.Headers;

namespace Notifo.SDK.Configuration;

internal sealed class AuthenticatingHttpMessageHandler : DelegatingHandler
{
    private readonly IAuthenticator authenticator;

    public AuthenticatingHttpMessageHandler(IAuthenticator authenticator)
    {
        InnerHandler = new HttpClientHandler();

        this.authenticator = authenticator;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization != null)
        {
            return base.SendAsync(request, cancellationToken);
        }

        if (!authenticator.ShouldIntercept(request))
        {
            return base.SendAsync(request, cancellationToken);
        }

        return InterceptAsync(request, true, cancellationToken);
    }

    private async Task<HttpResponseMessage> InterceptAsync(HttpRequestMessage request, bool retry,
        CancellationToken cancellationToken)
    {
        var token = await authenticator.GetBearerTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await authenticator.RemoveTokenAsync(token, cancellationToken);

            if (retry)
            {
                return await InterceptAsync(request, false, cancellationToken);
            }
        }

        return response;
    }
}
