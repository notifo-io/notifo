// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Mjml.Net;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email
{
    public class EmailTemplateFixture
    {
        public IImageFormatter ImageFormatter { get; } = new FakeFormatter();

        public IEmailFormatter EmailFormatter { get; private set; }

        public EmailTemplate EmailTemplate { get; private set; }

        public App App { get; } = new App();

        private sealed class FakeFormatter : IImageFormatter
        {
            public string Format(string? url, string preset)
            {
                return url ?? string.Empty;
            }

            public string? FormatWhenSet(string? url, string preset)
            {
                return url;
            }
        }

        public EmailTemplateFixture()
        {
            Task.Run(async () =>
            {
                Directory.CreateDirectory("_out");

                EmailFormatter = new EmailFormatter(ImageFormatter, new MjmlRenderer());
                EmailTemplate = await EmailFormatter.CreateInitialAsync();
            }).Wait();
        }
    }
}
