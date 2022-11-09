// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Text;

namespace Notifo.Infrastructure.Json;

public sealed class JsonInstantConverter : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();

        var parsed = InstantPattern.ExtendedIso.Parse(text!);

        if (parsed.Success)
        {
            return parsed.Value;
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return Instant.FromDateTimeOffset(result);
        }

        return parsed.Value;
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(InstantPattern.ExtendedIso.Format(value));
    }
}
