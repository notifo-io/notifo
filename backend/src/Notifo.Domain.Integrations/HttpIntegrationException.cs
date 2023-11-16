// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

[Serializable]
public class HttpIntegrationException<TResponse> : Exception where TResponse : class
{
    public TResponse? HttpResponse { get; }

    public int HttpStatusCode { get; }

    public HttpIntegrationException(string message, int statusCode = 400, TResponse? response = null, Exception? inner = null)
        : base(message, inner)
    {
        HttpResponse = response;
        HttpStatusCode = statusCode;
    }
}
