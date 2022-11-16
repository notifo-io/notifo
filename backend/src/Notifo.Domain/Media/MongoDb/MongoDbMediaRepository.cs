// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Media.MongoDb;

public sealed class MongoDbMediaRepository : MongoDbStore<MongoDbMedia>, IMediaRepository
{
    public MongoDbMediaRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Media";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoDbMedia> collection,
        CancellationToken ct = default)
    {
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDbMedia>(
                IndexKeys
                    .Ascending(x => x.Doc.AppId)
                    .Ascending(x => x.Doc.FileName)
                    .Descending(x => x.Doc.LastUpdate)),
            null, ct);
    }

    public async Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbMediaRepository/QueryAsync"))
        {
            var filter = BuildFilter(appId, query);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Doc.LastUpdate).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems.Select(x => x.ToMedia()));
        }
    }

    public async Task<Media?> GetAsync(string appId, string fileName,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbMediaRepository/GetAsync"))
        {
            var docId = MongoDbMedia.CreateId(appId, fileName);

            var document = await Collection.Find(x => x.DocId == docId).FirstOrDefaultAsync(ct);

            return document?.ToMedia();
        }
    }

    public async Task UpsertAsync(Media media,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbMediaRepository/UpsertAsync"))
        {
            var document = MongoDbMedia.FromMedia(media);

            await Collection.ReplaceOneAsync(x => x.DocId == document.DocId, document, UpsertReplace, ct);
        }
    }

    public async Task DeleteAsync(string appId, string fileName,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoDbMediaRepository/DeleteAsync"))
        {
            var docId = MongoDbMedia.CreateId(appId, fileName);

            await Collection.DeleteOneAsync(x => x.DocId == docId, ct);
        }
    }

    private static FilterDefinition<MongoDbMedia> BuildFilter(string appId, MediaQuery query)
    {
        var filters = new List<FilterDefinition<MongoDbMedia>>
        {
            Filter.Eq(x => x.Doc.AppId, appId)
        };

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var regex = new BsonRegularExpression(Regex.Escape(query.Query), "i");

            filters.Add(Filter.Regex(x => x.Doc.FileName, regex));
        }

        return Filter.And(filters);
    }
}
