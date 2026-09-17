// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

[Serializable]
public class HttpIntegrationException(string message, int statusCode = 400, Exception? inner = null) : Exception(message, inner)
{
    public int HttpStatusCode { get; } = statusCode;
}

[Serializable]
public class HttpIntegrationException<TResponse>(string message, int statusCode = 400, TResponse? response = null, Exception? inner = null) : HttpIntegrationException(message, statusCode, inner) where TResponse : class
{
    public TResponse? HttpResponse { get; } = response;
}
