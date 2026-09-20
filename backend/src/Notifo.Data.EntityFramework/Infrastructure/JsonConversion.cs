// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Notifo.Infrastructure;

public static class JsonConversion
{
    public static void UseJsonAttributes(this ModelBuilder builder, JsonSerializerOptions jsonOptions)
    {
        foreach (var entityType in builder.Model.GetEntityTypes().Select(x => x.ClrType).ToList())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Strings are already serialized, the attribute only documents them.
                if (property.GetCustomAttribute<JsonAttribute>() == null || property.PropertyType == typeof(string))
                {
                    continue;
                }

                var converterType = typeof(JsonValueConverter<>).MakeGenericType(property.PropertyType);

                // Configure the property explicitly, otherwise the type would be mapped as related entity.
                builder.Entity(entityType).Property(property.PropertyType, property.Name)
                    .HasConversion((ValueConverter)Activator.CreateInstance(converterType, jsonOptions)!);
            }
        }
    }

    public static void UseInstantAsDateTimeOffset(this ModelConfigurationBuilder builder)
    {
        builder.Properties<Instant>().HaveConversion<InstantConverter>();
        builder.Properties<Instant?>().HaveConversion<NullableInstantConverter>();
    }

    private sealed class JsonValueConverter<T>(JsonSerializerOptions jsonOptions) : ValueConverter<T, string>(
        v => JsonSerializer.Serialize(v, jsonOptions),
        v => JsonSerializer.Deserialize<T>(v, jsonOptions)!)
    {
    }

    private sealed class InstantConverter() : ValueConverter<Instant, DateTimeOffset>(
        v => v.ToDateTimeOffset(),
        v => Instant.FromDateTimeOffset(v))
    {
    }

    private sealed class NullableInstantConverter() : ValueConverter<Instant?, DateTimeOffset?>(
        v => v != null ? v.Value.ToDateTimeOffset() : null,
        v => v != null ? Instant.FromDateTimeOffset(v.Value) : null)
    {
    }
}
