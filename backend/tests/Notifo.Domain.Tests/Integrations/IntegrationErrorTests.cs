// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.Seven;
using Notifo.Domain.Integrations.Smtp;
using Notifo.Domain.Integrations.Telekom;
using Notifo.Domain.Integrations.Twilio;
using Notifo.Infrastructure;
using Twilio.Exceptions;

namespace Notifo.Domain.Integrations;

public class IntegrationErrorTests
{
    private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
    private HttpStatusCode statusCode = HttpStatusCode.OK;
    private string responseBody = "{}";

    public IntegrationErrorTests()
    {
        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient(new StaticHandler(() => (statusCode, responseBody))));
    }

    [Theory]
    [InlineData(SmtpStatusCode.ServiceNotAvailable, true)]
    [InlineData(SmtpStatusCode.MailboxBusy, true)]
    [InlineData(SmtpStatusCode.MailboxUnavailable, false)]
    [InlineData(SmtpStatusCode.AuthenticationRequired, false)]
    public void Should_detect_transient_smtp_command_errors(SmtpStatusCode code, bool isTransient)
    {
        var exception = new SmtpCommandException(SmtpErrorCode.UnexpectedStatusCode, code, "Error");

        Assert.Equal(isTransient, SmtpIntegration.IsTransient(exception));
    }

    [Fact]
    public void Should_detect_transient_smtp_connection_errors()
    {
        Assert.True(SmtpIntegration.IsTransient(new SmtpProtocolException("Error")));
        Assert.True(SmtpIntegration.IsTransient(new IOException()));
    }

    [Theory]
    [InlineData(429, true)]
    [InlineData(503, true)]
    [InlineData(400, false)]
    [InlineData(401, false)]
    public void Should_detect_transient_twilio_errors(int status, bool isTransient)
    {
        var exception = new ApiException(20000, status, "Error", string.Empty, null, null);

        Assert.Equal(isTransient, TwilioSmsIntegration.IsTransient(exception));
    }

    [Fact]
    public void Should_detect_transient_twilio_connection_errors()
    {
        Assert.True(TwilioSmsIntegration.IsTransient(new ApiConnectionException("Error")));
    }

    [Fact]
    public async Task Should_return_sent_if_telekom_request_succeeded()
    {
        var result = await SendTelekomAsync();

        Assert.Equal(DeliveryStatus.Sent, result.Status);
    }

    [Fact]
    public async Task Should_throw_domain_exception_if_telekom_request_failed_permanently()
    {
        statusCode = HttpStatusCode.BadRequest;

        await Assert.ThrowsAsync<DomainException>(SendTelekomAsync);
    }

    [Fact]
    public async Task Should_throw_transient_exception_if_telekom_request_failed_temporarily()
    {
        statusCode = HttpStatusCode.ServiceUnavailable;

        var exception = await Assert.ThrowsAsync<HttpIntegrationException>(SendTelekomAsync);

        Assert.True(exception.IsTransient());
    }

    [Fact]
    public async Task Should_throw_domain_exception_if_seven_request_failed_permanently()
    {
        statusCode = HttpStatusCode.Unauthorized;

        await Assert.ThrowsAsync<DomainException>(SendSevenAsync);
    }

    [Fact]
    public async Task Should_throw_transient_exception_if_seven_request_failed_temporarily()
    {
        statusCode = HttpStatusCode.TooManyRequests;

        var exception = await Assert.ThrowsAsync<HttpIntegrationException<object>>(SendSevenAsync);

        Assert.True(exception.IsTransient());
    }

    private Task<DeliveryResult> SendTelekomAsync()
    {
        var sut = new TelekomSmsIntegration(httpClientFactory);

        var context = CreateContext(new Dictionary<string, string>
        {
            [TelekomSmsIntegration.ApiKeyProperty.Name] = "key",
            [TelekomSmsIntegration.PhoneNumberProperty.Name] = "4912345678"
        });

        return sut.SendAsync(context, CreateMessage(), default);
    }

    private Task<DeliveryResult> SendSevenAsync()
    {
        var sut = new SevenSmsIntegration(new SevenSmsClientPool(new MemoryCache(Options.Create(new MemoryCacheOptions())), httpClientFactory));

        var context = CreateContext(new Dictionary<string, string>
        {
            [SevenSmsIntegration.ApiKeyProperty.Name] = Guid.NewGuid().ToString(),
            [SevenSmsIntegration.FromProperty.Name] = "Notifo"
        });

        return sut.SendAsync(context, CreateMessage(), default);
    }

    private static SmsMessage CreateMessage()
    {
        return new SmsMessage
        {
            To = "4987654321",
            Text = "Text",
            TrackingToken = "token"
        };
    }

    private static IntegrationContext CreateContext(Dictionary<string, string> properties)
    {
        return new IntegrationContext
        {
            AppId = "app",
            AppName = "app",
            CallbackToken = string.Empty,
            CallbackUrl = string.Empty,
            IntegrationAdapter = A.Fake<IIntegrationAdapter>(),
            IntegrationId = "integration",
            Properties = properties,
            UpdateStatusAsync = (_, _) => Task.CompletedTask,
            WebhookUrl = "https://notifo.io/webhook"
        };
    }

    private sealed class StaticHandler(Func<(HttpStatusCode, string)> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var (status, body) = response();

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
