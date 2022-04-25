// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;
using FakeItEasy;
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
        private readonly App app = new App("1", default);
        private readonly IEmailFormatter emailFormatterNormal = new EmailFormatterNormal(A.Fake<IImageFormatter>(), A.Fake<IEmailUrl>(), new MjmlRenderer());
        private readonly IEmailFormatter emailFormatterLiquid = new EmailFormatterLiquid(A.Fake<IImageFormatter>(), A.Fake<IEmailUrl>(), new MjmlRenderer());
        private readonly EmailTemplate emailTemplateNormal;
        private readonly EmailTemplate emailTemplateLiquid;
        private readonly User user = new User("1", "1", default);

        public EmailFormatterBenchmarks()
        {
            emailTemplateNormal = emailFormatterNormal.CreateInitialAsync().AsTask().Result;
            emailTemplateLiquid = emailFormatterLiquid.CreateInitialAsync().AsTask().Result;
        }

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatNormal()
        {
            return await emailFormatterNormal.FormatAsync(EmailJob.ForPreview, emailTemplateNormal, app, user);
        }

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatLiquid()
        {
            return await emailFormatterLiquid.FormatAsync(EmailJob.ForPreview, emailTemplateLiquid, app, user);
        }
    }
}
