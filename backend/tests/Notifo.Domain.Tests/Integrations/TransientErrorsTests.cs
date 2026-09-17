// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;

namespace Notifo.Domain.Integrations;

public class TransientErrorsTests
{
    public static readonly TheoryData<Exception> TransientExceptions = new TheoryData<Exception>
    {
        new OperationCanceledException(),
        new TaskCanceledException(),
        new TimeoutException(),
        new IOException(),
        new SocketException(),
        new HttpRequestException("Network"),
        new HttpRequestException("Server", null, HttpStatusCode.InternalServerError),
        new HttpRequestException("Rate limit", null, HttpStatusCode.TooManyRequests),
        new HttpIntegrationException("Server", 503),
        new HttpIntegrationException<object>("Timeout", 408),
        new AggregateException(new TimeoutException())
    };

    public static readonly TheoryData<Exception> PermanentExceptions = new TheoryData<Exception>
    {
        new InvalidOperationException(),
        new NotSupportedException(),
        new HttpRequestException("Not found", null, HttpStatusCode.NotFound),
        new HttpIntegrationException("Bad request", 400),
        new HttpIntegrationException<object>("Unauthorized", 401)
    };

    [Theory]
    [MemberData(nameof(TransientExceptions))]
    public void Should_detect_transient_exception(Exception exception)
    {
        Assert.True(exception.IsTransient());
    }

    [Theory]
    [MemberData(nameof(PermanentExceptions))]
    public void Should_detect_permanent_exception(Exception exception)
    {
        Assert.False(exception.IsTransient());
    }
}
