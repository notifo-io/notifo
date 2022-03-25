// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;
using Mjml.Net;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class EmailFormatterBenchmarks
    {
        private readonly App app = new App();
        private readonly IEmailFormatter emailFormatterNormal = new EmailFormatterNormal(new FakeImageFormatter(), new MjmlRenderer());
        private readonly IEmailFormatter emailFormatterLiquid = new EmailFormatterLiquid(new FakeImageFormatter(), new MjmlRenderer());
        private readonly EmailTemplate emailTemplateNormal;
        private readonly EmailTemplate emailTemplateLiquid;

        public EmailFormatterBenchmarks()
        {
            emailTemplateNormal = emailFormatterNormal.CreateInitialAsync().AsTask().Result;
            emailTemplateLiquid = emailFormatterLiquid.CreateInitialAsync().AsTask().Result;
        }

        private sealed class FakeImageFormatter : IImageFormatter
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

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatNormal()
        {
            return await emailFormatterNormal.FormatAsync(EmailJob.ForPreview, emailTemplateNormal, app, new User());
        }

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatLiquid()
        {
            return await emailFormatterLiquid.FormatAsync(EmailJob.ForPreview, emailTemplateLiquid, app, new User());
        }
    }
}
