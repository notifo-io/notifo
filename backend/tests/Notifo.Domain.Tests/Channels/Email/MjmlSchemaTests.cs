// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Domain.Channels.Email.Formatting;
using System.Text.Json;

namespace Notifo.Domain.Channels.Email;

[UsesVerify]
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
        var json = JsonSerializer.Serialize(MjmlSchema.Build(mjmlRenderer));

        await Verify(new { json });
    }
}
