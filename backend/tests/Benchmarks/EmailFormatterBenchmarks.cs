// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Mjml.Net;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Utils;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class EmailFormatterBenchmarks
    {
        private readonly IEmailFormatter emailFormatterNormal = new EmailFormatterNormal(A.Fake<IImageFormatter>(), A.Fake<IEmailUrl>(), new MjmlRenderer());
        private readonly IEmailFormatter emailFormatterLiquid = new EmailFormatterLiquid(A.Fake<IImageFormatter>(), A.Fake<IEmailUrl>(), new MjmlRenderer());
        private readonly EmailTemplate emailTemplateNormal;
        private readonly EmailTemplate emailTemplateLiquid;

        public EmailFormatterBenchmarks()
        {
            emailTemplateNormal = emailFormatterNormal.CreateInitialAsync().AsTask().Result;
            emailTemplateLiquid = emailFormatterLiquid.CreateInitialAsync().AsTask().Result;
        }

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatNormal()
        {
            return await emailFormatterNormal.FormatAsync(emailTemplateNormal, PreviewData.Jobs, PreviewData.App, PreviewData.User);
        }

        [Benchmark]
        public async ValueTask<FormattedEmail> FormatLiquid()
        {
            return await emailFormatterLiquid.FormatAsync(emailTemplateLiquid, PreviewData.Jobs, PreviewData.App, PreviewData.User);
        }
    }
}
