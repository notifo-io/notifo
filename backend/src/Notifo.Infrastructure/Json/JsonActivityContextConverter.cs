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
    public sealed class JsonActivityContextConverter : JsonConverter<ActivityContext>
    {
        private static readonly JsonEncodedText IsRemote = JsonEncodedText.Encode("isRemote");
        private static readonly JsonEncodedText TraceId = JsonEncodedText.Encode("traceId");
        private static readonly JsonEncodedText TraceFlags = JsonEncodedText.Encode("traceFlags");
        private static readonly JsonEncodedText TraceState = JsonEncodedText.Encode("traceState");
        private static readonly JsonEncodedText SpanId = JsonEncodedText.Encode("spanId");

        public override ActivityContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return default;
                case JsonTokenType.StartObject:
                    var isRemote = false;
                    var traceId = default(ActivityTraceId);
                    var traceFlags = default(ActivityTraceFlags);
                    var traceState = (string?)null;
                    var spanId = default(ActivitySpanId);

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        var property = reader.GetString();

                        reader.Read();

                        switch (property)
                        {
                            case "isRemote":
                                isRemote = reader.GetBoolean();
                                break;
                            case "traceId":
                                traceId = ActivityTraceId.CreateFromString(reader.GetString());
                                break;
                            case "traceFlags":
                                traceFlags = Enum.Parse<ActivityTraceFlags>(reader.GetString()!);
                                break;
                            case "traceState":
                                traceState = reader.GetString();
                                break;
                            case "spanId":
                                spanId = ActivitySpanId.CreateFromString(reader.GetString());
                                break;
                            default:
                                ThrowHelper.JsonException($"Invalid property {property}.");
                                return default;
                        }
                    }

                    return new ActivityContext(traceId, spanId, traceFlags, traceState, isRemote);
                default:
                    ThrowHelper.JsonException($"Expected JsonTokenType.StartObject or JsonTokenType.Null, got {reader.TokenType}.");
                    return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, ActivityContext value, JsonSerializerOptions options)
        {
            if (value == default)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            if (value.IsRemote)
            {
                writer.WriteBoolean(IsRemote, value.IsRemote);
            }

            if (value.TraceId != default)
            {
                writer.WriteString(TraceId, value.TraceId.ToString());
            }

            if (value.TraceFlags != default)
            {
                writer.WriteString(TraceFlags, value.TraceFlags.ToString());
            }

            if (value.TraceState != null)
            {
                writer.WriteString(TraceState, value.TraceState);
            }

            if (value.SpanId != default)
            {
                writer.WriteString(SpanId, value.SpanId.ToString());
            }

            writer.WriteEndObject();
        }
    }
}
