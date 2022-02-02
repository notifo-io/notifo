// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Notifo.Infrastructure.MongoDb
{
    public class MongoDbStore<T> : MongoDbRepository<T> where T : MongoDbEntity
    {
        public MongoDbStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected async Task<T?> GetDocumentAsync(string id,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(id);

            var existing =
                await Collection.Find(x => x.DocId == id)
                    .FirstOrDefaultAsync(ct);

            return existing;
        }

        protected async Task UpsertDocumentAsync(string id, T value, string? oldEtag,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(id);
            Guard.NotNull(value);

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
                            throw new InconsistentStateException(existingVersion["e"].AsString, oldEtag, ex);
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

        public Task DeleteAsync(string id,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(id);

            return Collection.DeleteOneAsync(x => x.DocId == id, ct);
        }
    }
}
