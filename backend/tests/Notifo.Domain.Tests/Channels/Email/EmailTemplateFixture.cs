// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mjml.AspNetCore;
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

                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMjmlServices();

                var mjmlServices =
                    serviceCollection.BuildServiceProvider()
                         .GetRequiredService<IMjmlServices>();

                EmailFormatter = new EmailFormatter(ImageFormatter, mjmlServices);
                EmailTemplate = await EmailFormatter.CreateInitialAsync();
            }).Wait();
        }
    }
}
