// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

namespace Notifo.Domain.Integrations.Http;

public class HttpIntegrationTests
{
    private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
    private readonly HttpIntegration sut;
    private HttpStatusCode statusCode = HttpStatusCode.OK;

    public HttpIntegrationTests()
    {
        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient(new StaticHandler(() => statusCode)));

        sut = new HttpIntegration(httpClientFactory);
    }

    [Fact]
    public async Task Should_return_handled_if_request_succeeded()
    {
        var result = await sut.SendAsync(CreateContext(), new WebhookMessage { Payload = new { } }, default);

        Assert.Equal(DeliveryStatus.Handled, result.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Should_return_failed_if_request_failed_permanently(HttpStatusCode code)
    {
        statusCode = code;

        var result = await sut.SendAsync(CreateContext(), new WebhookMessage { Payload = new { } }, default);

        Assert.Equal(DeliveryStatus.Failed, result.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Should_throw_exception_if_request_failed_temporarily(HttpStatusCode code)
    {
        statusCode = code;

        await Assert.ThrowsAsync<HttpIntegrationException>(() => sut.SendAsync(CreateContext(), new WebhookMessage { Payload = new { } }, default));
    }

    private static IntegrationContext CreateContext()
    {
        return new IntegrationContext
        {
            AppId = "app",
            AppName = "app",
            CallbackToken = string.Empty,
            CallbackUrl = string.Empty,
            IntegrationAdapter = A.Fake<IIntegrationAdapter>(),
            IntegrationId = "integration",
            Properties = new Dictionary<string, string>
            {
                ["Url"] = "https://webhook.example.com",
                ["Method"] = "POST",
                ["SendAlways"] = "true",
                ["SendConfirm"] = "false"
            },
            UpdateStatusAsync = (_, _) => Task.CompletedTask,
            WebhookUrl = string.Empty
        };
    }

    private sealed class StaticHandler(Func<HttpStatusCode> statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode()));
        }
    }
}
