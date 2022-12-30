// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Utils;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class EmailFormatterBenchmarks
{
    private readonly IEmailFormatter emailFormatterLiquid = new EmailFormatterLiquid(A.Fake<IImageFormatter>(), A.Fake<IEmailUrl>());
    private readonly EmailTemplate emailTemplateLiquid;

    public EmailFormatterBenchmarks()
    {
        emailTemplateLiquid = emailFormatterLiquid.CreateInitialAsync().AsTask().Result;
    }

    [Benchmark]
    public async ValueTask<FormattedEmail> FormatLiquid()
    {
        return await emailFormatterLiquid.FormatAsync(emailTemplateLiquid, PreviewData.Jobs, PreviewData.App, PreviewData.User);
    }
}
