// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Collections.Bson;
using Notifo.Infrastructure.Collections.Json;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.TestHelpers;

public static class TestUtils
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions();

    static TestUtils()
    {
        DefaultOptions.Converters.Add(new JsonReadonlyListConverterFactory());
        DefaultOptions.Converters.Add(new JsonReadonlyDictionaryConverterFactory());
        DefaultOptions.Converters.Add(new JsonActivityContextConverter());
        DefaultOptions.Converters.Add(new JsonActivitySpanIdConverter());
        DefaultOptions.Converters.Add(new JsonActivityTraceIdConverter());
    }

    public sealed class ObjectHolder<T>
    {
        [BsonRequired]
        public T Value { get; set; }
    }

    public static T SerializeAndDeserialize<T>(this T value)
    {
        var obj = new ObjectHolder<T>
        {
            Value = value
        };

        var json = JsonSerializer.Serialize(obj, DefaultOptions);

        return JsonSerializer.Deserialize<ObjectHolder<T>>(json, DefaultOptions)!.Value;
    }

    public static T SerializeAndDeserializeBson<T>(this T value)
    {
        var obj = new ObjectHolder<T>
        {
            Value = value
        };

        var stream = new MemoryStream();

        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, obj);

            writer.Flush();
        }

        stream.Position = 0;

        using (var reader = new BsonBinaryReader(stream))
        {
            var result = BsonSerializer.Deserialize<ObjectHolder<T>>(reader);

            return result.Value;
        }
    }

    public static TOut SerializeAndDeserializeBson<TIn, TOut>(this TIn value)
    {
        var obj = new ObjectHolder<TIn>
        {
            Value = value
        };

        var stream = new MemoryStream();

        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, obj);

            writer.Flush();
        }

        stream.Position = 0;

        using (var reader = new BsonBinaryReader(stream))
        {
            var result = BsonSerializer.Deserialize<ObjectHolder<TOut>>(reader);

            return result.Value;
        }
    }

    public static T Deserialize<T>(string value)
    {
        var json = $"{{ \"Value\": \"{value}\" }}";

        return JsonSerializer.Deserialize<ObjectHolder<T>>(json, DefaultOptions)!.Value;
    }

    public static T Deserialize<T>(object value)
    {
        var json = $"{{ \"Value\": {value} }}";

        return JsonSerializer.Deserialize<ObjectHolder<T>>(json, DefaultOptions)!.Value;
    }

    public static T Deserialize<T>(string value, JsonConverter converter)
    {
        var options = new JsonSerializerOptions();

        options.Converters.Add(converter);

        var json = $"{{ \"Value\": \"{value}\" }}";

        return JsonSerializer.Deserialize<ObjectHolder<T>>(json, options)!.Value;
    }
}
