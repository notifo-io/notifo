// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;
using Notifo.Domain.Utils;

namespace Benchmarks;

public class FormatterTests
{
    private readonly string template = "Hello {userFirstName} {userLastName}";
    private readonly Dictionary<string, string?> properties = new Dictionary<string, string?>
    {
        ["userFirstName"] = "Donald",
        ["userLastName"] = "Duck"
    };

    [Benchmark]
    public string FormatNormal()
    {
        return template.Format(properties);
    }

    [Benchmark]
    public string FormatLiquid()
    {
        return template.FormatLiquid(properties);
    }
}
