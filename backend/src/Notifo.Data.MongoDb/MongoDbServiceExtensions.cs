// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
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

                return MongoClientFactory.Create(connectionString, settings =>
                {
                    settings.ClusterConfigurator = builder =>
                    {
                        builder.Subscribe(new DiagnosticsActivityEventSubscriber());
                    };
                });
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
