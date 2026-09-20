// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Notifo.Infrastructure.Collections.Json;
using Notifo.Infrastructure.Json;

namespace Notifo.Infrastructure;

public static class TestHelpers
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions();

    static TestHelpers()
    {
        DefaultOptions.Converters.Add(new JsonReadonlyListConverterFactory());
        DefaultOptions.Converters.Add(new JsonReadonlyDictionaryConverterFactory());
        DefaultOptions.Converters.Add(new JsonActivityContextConverter());
        DefaultOptions.Converters.Add(new JsonActivitySpanIdConverter());
        DefaultOptions.Converters.Add(new JsonActivityTraceIdConverter());
    }

    public sealed class ObjectHolder<T>
    {
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
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        var options = new JsonSerializerOptions();
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

        options.Converters.Add(converter);

        var json = $"{{ \"Value\": \"{value}\" }}";

        return JsonSerializer.Deserialize<ObjectHolder<T>>(json, options)!.Value;
    }
}
