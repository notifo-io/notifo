// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Infrastructure.Collections.Json
{
    public sealed class JsonReadonlyListConverterFactory : JsonConverterFactory
    {
        private sealed class Converter<T> : JsonConverter<ReadonlyList<T>>
        {
            private readonly Type innerType = typeof(IReadOnlyList<T>);

            public override ReadonlyList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var inner = JsonSerializer.Deserialize<List<T>>(ref reader, options)!;

                return new ReadonlyList<T>(inner);
            }

            public override void Write(Utf8JsonWriter writer, ReadonlyList<T> value, JsonSerializerOptions options)
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

            if (typeToConvert.GetGenericTypeDefinition() != typeof(ReadonlyList<>))
            {
                return false;
            }

            return true;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var concreteType = typeof(Converter<>).MakeGenericType(
                new Type[]
                {
                    typeToConvert.GetGenericArguments()[0]
                });

            var converter = (JsonConverter)Activator.CreateInstance(concreteType)!;

            return converter;
        }
    }
}
