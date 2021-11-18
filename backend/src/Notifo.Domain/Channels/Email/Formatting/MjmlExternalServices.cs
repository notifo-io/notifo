// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;
using Mjml.AspNetCore;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed class MjmlExternalServices : IMjmlServices
    {
        private sealed class Request
        {
            public string Mjml { get; set; }
        }

        public async Task<MjmlResponse> Render(string view, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(view))
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new Request
                    {
                        Mjml = view
                    };

                    const string url = "https://mjml.notifo.io";

                    var response = await httpClient.PostAsJsonAsync(url, request, token);
                    var responseJson = await response.Content.ReadFromJsonAsync<MjmlResponse>(cancellationToken: token);

                    return responseJson!;
                }
            }

            return new MjmlResponse();
        }

        public Task<MjmlResponse> RenderFromJson(string json, CancellationToken token = default)
        {
            throw new NotSupportedException();
        }
    }
}
