// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Notifo.SDK;

public class HttpClientTests
{
    private record NoopOptions : StaticNotifoOptions
    {
        public override void Validate()
        {
        }
    }

    [Fact]
    public async Task Should_make_request_with_empty_options()
    {
        var client =
            NotifoClientBuilder.Create()
                .SetOptions(new NoopOptions())
                .Build();

        using (var httpClient = client.CreateHttpClient())
        {
            var response = await httpClient.GetAsync("https://notifo.io");

            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task Should_survive_dispose()
    {
        var client =
            NotifoClientBuilder.Create()
                .SetApiKey("Key")
                .Build();

        using (var httpClient = client.CreateHttpClient())
        {
            var response = await httpClient.GetAsync("https://notifo.io");

            response.EnsureSuccessStatusCode();
        }

        using (var httpClient = client.CreateHttpClient())
        {
            var response = await httpClient.GetAsync("https://notifo.io");

            response.EnsureSuccessStatusCode();
        }
    }
}
