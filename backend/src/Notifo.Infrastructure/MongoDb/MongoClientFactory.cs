// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Collections.Bson;

namespace Notifo.Infrastructure.MongoDb;

public static class MongoClientFactory
{
    public static MongoClient Create(string? connectionString, Action<MongoClientSettings>? configure = null)
    {
        RegisterDefaultSerializers();

        ConventionRegistry.Register("EnumStringConvention", new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String)
        }, t => true);

        ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        }, t => true);

        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

        // The current version of the linq provider has some issues with base classes.
        clientSettings.LinqProvider = LinqProvider.V2;

        // If we really need custom config.
        configure?.Invoke(clientSettings);

        return new MongoClient(clientSettings);
    }

    public static void RegisterDefaultSerializers()
    {
        // Allow all types, independent from the actual assembly.
        BsonSerializer.TryRegisterSerializer(new ObjectSerializer(type => true));

        ActivityContextSerializer.Register();
        ActivitySpanIdSerializer.Register();
        ActivityTraceIdSerializer.Register();
        DurationSerializer.Register();
        InstantSerializer.Register();
        LocalDateSerializer.Register();
        LocalTimeSerializer.Register();

        BsonSerializer.RegisterGenericSerializerDefinition(
            typeof(ReadonlyList<>),
            typeof(ReadonlyListSerializer<>));

        BsonSerializer.RegisterGenericSerializerDefinition(
            typeof(ReadonlyDictionary<,>),
            typeof(ReadonlyDictionarySerializer<,>));
    }
}
