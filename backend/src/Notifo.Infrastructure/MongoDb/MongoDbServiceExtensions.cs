// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Notifo.Infrastructure.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbServiceExtensions
    {
        public static void AddMyMongoDb(this IServiceCollection services, IConfiguration config)
        {
            ActivityContextSerializer.Register();
            ActivitySpanIdSerializer.Register();
            ActivityTraceIdSerializer.Register();
            DurationSerializer.Register();
            InstantSerializer.Register();
            LocalDateSerializer.Register();
            LocalTimeSerializer.Register();

            ConventionRegistry.Register("EnumStringConvention", new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            }, t => true);

            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            }, t => true);

            services.ConfigureAndValidate<MongoDbOptions>(config, "storage:mongoDb");

            services.AddSingletonAs(c =>
                {
                    var connectionString = c.GetRequiredService<IOptions<MongoDbOptions>>().Value.ConnectionString;

                    var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

                    clientSettings.ClusterConfigurator = builder =>
                    {
                        builder.Subscribe(new DiagnosticsActivityEventSubscriber());
                    };

                    return new MongoClient(clientSettings);
                })
                .As<IMongoClient>();

            services.AddSingletonAs(c =>
                {
                    var databaseName = c.GetRequiredService<IOptions<MongoDbOptions>>().Value.DatabaseName;

                    return c.GetRequiredService<IMongoClient>().GetDatabase(databaseName);
                })
                .As<IMongoDatabase>();
        }
    }
}
