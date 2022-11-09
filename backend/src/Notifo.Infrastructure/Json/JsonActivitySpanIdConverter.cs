// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Infrastructure.Json;

public sealed class JsonActivitySpanIdConverter : JsonConverter<ActivitySpanId>
{
    public override ActivitySpanId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.String:
                var text = reader.GetString();

                return ActivitySpanId.CreateFromString(text);
            default:
                ThrowHelper.JsonException($"Expected JsonTokenType.String or JsonTokenType.Null, got {reader.TokenType}.");
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, ActivitySpanId value, JsonSerializerOptions options)
    {
        if (value == default)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
