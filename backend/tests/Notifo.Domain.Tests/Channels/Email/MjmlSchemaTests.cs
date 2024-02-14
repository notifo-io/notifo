﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Mjml.Net;
using Notifo.Domain.Channels.Email.Formatting;

namespace Notifo.Domain.Channels.Email;

public class MjmlSchemaTests
{
    private readonly IMjmlRenderer mjmlRenderer = new MjmlRenderer();

    [Fact]
    public async Task Should_build_schema()
    {
        var schema = MjmlSchema.Build(mjmlRenderer);

        await Verify(schema);
    }

    [Fact]
    public async Task Should_build_schema_as_json()
    {
        var json = JsonSerializer.Serialize(MjmlSchema.Build(mjmlRenderer), new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await Verify(json);
    }
}
