// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MongoDB.Driver.Linq;
using Notifo.Infrastructure.MongoDb;

namespace Microsoft.Extensions.DependencyInjection;

public static class MongoDbServiceExtensions
{
    public static void AddMyMongoDb(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureAndValidate<MongoDbOptions>(config, "storage:mongoDb");

        services.AddSingletonAs(c =>
            {
                var connectionString = c.GetRequiredService<IOptions<MongoDbOptions>>().Value.ConnectionString;

                var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

                // The current version of the linq provider has some issues with base classes.
                clientSettings.LinqProvider = LinqProvider.V2;
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
