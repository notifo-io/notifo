// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Infrastructure.Json
{
    public sealed class JsonActivityTraceIdConverter : JsonConverter<ActivityTraceId>
    {
        public override ActivityTraceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();

            return ActivityTraceId.CreateFromString(text);
        }

        public override void Write(Utf8JsonWriter writer, ActivityTraceId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
