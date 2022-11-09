// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;

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

    protected HttpIntegrationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        HttpResponse = info.GetValue(nameof(HttpResponse), typeof(TResponse)) as TResponse;
        HttpStatusCode = info.GetInt32(nameof(HttpStatusCode));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(HttpResponse), HttpResponse);
        info.AddValue(nameof(HttpStatusCode), HttpStatusCode);
    }
}
