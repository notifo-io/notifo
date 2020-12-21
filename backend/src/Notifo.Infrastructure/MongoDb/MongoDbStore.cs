// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Notifo.Infrastructure.MongoDb
{
    public class MongoDbStore<T> : MongoDbRepository<T> where T : MongoDbEntity
    {
        public MongoDbStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected async Task<T?> GetDocumentAsync(string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var existing =
                await Collection.Find(x => x.DocId == id)
                    .FirstOrDefaultAsync(ct);

            return existing;
        }

        protected async Task UpsertDocumentAsync(string id, T value, string? oldEtag = null, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(value, nameof(value));

            try
            {
                if (!string.IsNullOrWhiteSpace(oldEtag))
                {
                    await Collection.ReplaceOneAsync(x => x.DocId == id && x.Etag == oldEtag, value, UpsertReplace, ct);
                }
                else
                {
                    await Collection.ReplaceOneAsync(x => x.DocId == id, value, UpsertReplace, ct);
                }
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    if (oldEtag != null)
                    {
                        var existingVersion =
                            await Collection.Find(x => x.DocId == id).Only(x => x.DocId, x => x.Etag)
                                .FirstOrDefaultAsync(ct);

                        if (existingVersion != null)
                        {
                            throw new InconsistentStateException(existingVersion[nameof(MongoDbEntity.Etag)].AsString, oldEtag, ex);
                        }
                    }

                    throw new UniqueConstraintException();
                }
                else
                {
                    throw;
                }
            }
        }

        public Task DeleteAsync(string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return Collection.DeleteOneAsync(x => x.DocId == id, ct);
        }
    }
}
