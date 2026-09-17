// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;

namespace Notifo.Domain.Integrations;

public static class TransientErrors
{
    public static bool IsTransient(this Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => true,
            TimeoutException => true,
            IOException => true,
            SocketException => true,
            HttpRequestException http => http.StatusCode == null || IsTransient(http.StatusCode.Value),
            HttpIntegrationException http => IsTransient((HttpStatusCode)http.HttpStatusCode),
            AggregateException aggregate => aggregate.InnerExceptions.Any(IsTransient),
            _ => false
        };
    }

    public static bool IsTransient(this HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout || (int)statusCode >= 500;
    }
}
