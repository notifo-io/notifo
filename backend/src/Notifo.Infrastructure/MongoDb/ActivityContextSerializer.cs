// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Notifo.Infrastructure.MongoDb
{
    public sealed class ActivityContextSerializer : SerializerBase<ActivityContext>
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new ActivityContextSerializer());
            }
        }

        public override ActivityContext Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            switch (reader.CurrentBsonType)
            {
                case BsonType.Null:
                    reader.ReadNull();
                    return default;
                case BsonType.Document:
                    reader.ReadStartDocument();

                    var isRemote = false;
                    var traceId = default(ActivityTraceId);
                    var traceFlags = default(ActivityTraceFlags);
                    var traceState = (string?)null;
                    var spanId = default(ActivitySpanId);

                    while (reader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var name = reader.ReadName(Utf8NameDecoder.Instance);

                        switch (name)
                        {
                            case nameof(ActivityContext.IsRemote):
                                isRemote = reader.ReadBoolean();
                                break;
                            case nameof(ActivityContext.TraceId):
                                traceId = ActivityTraceId.CreateFromString(reader.ReadString());
                                break;
                            case nameof(ActivityContext.TraceFlags):
                                traceFlags = Enum.Parse<ActivityTraceFlags>(reader.ReadString()!);
                                break;
                            case nameof(ActivityContext.TraceState):
                                traceState = reader.ReadString();
                                break;
                            case nameof(ActivityContext.SpanId):
                                spanId = ActivitySpanId.CreateFromString(reader.ReadString());
                                break;
                            default:
                                ThrowHelper.BsonSerializationException($"Invalid property {name}.");
                                return default;
                        }
                    }

                    reader.ReadEndDocument();

                    return new ActivityContext(traceId, spanId, traceFlags, traceState, isRemote);
                default:
                    ThrowHelper.BsonSerializationException($"Expected BsonType.Document or JsonTokenType.Null, got {reader.CurrentBsonType}.");
                    return default;
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ActivityContext value)
        {
            var writer = context.Writer;

            if (value == default)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartDocument();

            if (value.IsRemote)
            {
                writer.WriteName(nameof(value.IsRemote));
                writer.WriteBoolean(value.IsRemote);
            }

            if (value.TraceId != default)
            {
                writer.WriteName(nameof(value.TraceId));
                writer.WriteString(value.TraceId.ToString());
            }

            if (value.TraceFlags != default)
            {
                writer.WriteName(nameof(value.TraceFlags));
                writer.WriteString(value.TraceFlags.ToString());
            }

            if (value.TraceState != null)
            {
                writer.WriteName(nameof(value.TraceState));
                writer.WriteString(value.TraceState);
            }

            if (value.SpanId != default)
            {
                writer.WriteName(nameof(value.SpanId));
                writer.WriteString(value.SpanId.ToString());
            }

            writer.WriteEndDocument();
        }
    }
}
