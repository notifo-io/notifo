// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;

namespace Notifo.SDK
{
    public sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public static readonly DateTimeOffsetConverter Instance = new DateTimeOffsetConverter();

        private DateTimeOffsetConverter()
        {
        }

        public override DateTimeOffset ReadJson(JsonReader reader, Type objectType, [AllowNull] DateTimeOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return DateTimeOffset.Parse(reader.Value.ToString()!);
            }

            if (reader.TokenType == JsonToken.Date)
            {
                return ((DateTime)reader.Value);
            }

            throw new JsonException($"Not a valid date time, expected String or Date, but got {reader.TokenType}.");
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] DateTimeOffset value, JsonSerializer serializer)
        {
            writer.WriteValue(value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        }
    }
}
