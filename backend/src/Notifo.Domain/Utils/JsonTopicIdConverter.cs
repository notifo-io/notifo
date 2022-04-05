// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Domain.Utils
{
    public sealed class JsonTopicIdConverter : JsonConverter<TopicId>
    {
        public override TopicId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString()!;
        }

        public override void Write(Utf8JsonWriter writer, TopicId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
