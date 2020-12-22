// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Notifo.Infrastructure.Configuration;
using Notifo.Infrastructure.Initialization;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Notifo.Infrastructure.MongoDb
{
    public abstract class MongoDbRepository<TEntity> : IInitializable
    {
        private const string CollectionFormat = "{0}Set";

        protected static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        protected static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> IndexKeys = Builders<TEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<TEntity> Projection = Builders<TEntity>.Projection;

        private readonly IMongoDatabase mongoDatabase;
        private IMongoCollection<TEntity> mongoCollection;

        protected IMongoCollection<TEntity> Collection
        {
            get
            {
                if (mongoCollection == null)
                {
                    throw new InvalidOperationException("Collection has not been initialized yet.");
                }

                return mongoCollection;
            }
        }

        protected IMongoDatabase Database
        {
            get { return mongoDatabase; }
        }

        protected MongoDbRepository(IMongoDatabase database)
        {
            mongoDatabase = database;
        }

        protected virtual MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings();
        }

        protected virtual string CollectionName()
        {
            return string.Format(CultureInfo.InvariantCulture, CollectionFormat, typeof(TEntity).Name);
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ClearAsync()
        {
            try
            {
                await Database.DropCollectionAsync(CollectionName());
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code != 26)
                {
                    throw;
                }
            }

            await InitializeAsync(default);
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            try
            {
                CreateCollection();

                await SetupCollectionAsync(Collection, ct);
            }
            catch (Exception ex)
            {
                var error = new ConfigurationError($"MongoDb connection failed to connect to database {Database.DatabaseNamespace.DatabaseName}");

                throw new ConfigurationException(error, ex);
            }
        }

        private void CreateCollection()
        {
            mongoCollection = mongoDatabase.GetCollection<TEntity>(
                CollectionName(),
                CollectionSettings() ?? new MongoCollectionSettings());
        }
    }
}