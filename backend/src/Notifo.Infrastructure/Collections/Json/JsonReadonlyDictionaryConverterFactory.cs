// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Infrastructure.Collections.Json
{
    public sealed class JsonReadonlyDictionaryConverterFactory : JsonConverterFactory
    {
        private sealed class Converter<TKey, TValue> : JsonConverter<ReadonlyDictionary<TKey, TValue>> where TKey : notnull
        {
            private readonly Type innerType = typeof(IReadOnlyDictionary<TKey, TValue>);

            public override ReadonlyDictionary<TKey, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var inner = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)!;

                return new ReadonlyDictionary<TKey, TValue>(inner);
            }

            public override void Write(Utf8JsonWriter writer, ReadonlyDictionary<TKey, TValue> value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, innerType, options);
            }
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(ReadonlyDictionary<,>))
            {
                return false;
            }

            return true;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var concreteType = typeof(Converter<,>).MakeGenericType(
                new Type[]
                {
                    typeToConvert.GetGenericArguments()[0],
                    typeToConvert.GetGenericArguments()[1]
                });

            var converter = (JsonConverter)Activator.CreateInstance(concreteType)!;

            return converter;
        }
    }
}
