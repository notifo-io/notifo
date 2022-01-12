// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Notifo.Infrastructure.MongoDb
{
    public static class MongoDbExtensions
    {
        public static IFindFluent<TDocument, TDocument> Page<TDocument>(this IFindFluent<TDocument, TDocument> find, QueryBase query)
        {
            return find.Skip(query.Skip).Limit(query.Take);
        }

        public static Task<List<TDocument>> ToListAsync<TDocument>(this IFindFluent<TDocument, TDocument> find, QueryBase query,
            CancellationToken ct)
        {
            return find.Skip(query.Skip).Limit(query.Take).ToListAsync(ct);
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include1,
            Expression<Func<TDocument, object?>> include2)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include1,
            Expression<Func<TDocument, object?>> include2,
            Expression<Func<TDocument, object?>> include3)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2).Include(include3));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude1,
            Expression<Func<TDocument, object?>> exclude2)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude1,
            Expression<Func<TDocument, object?>> exclude2,
            Expression<Func<TDocument, object?>> exclude3)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2).Exclude(exclude3));
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IFindFluent<T, T> find,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var cursor = await find.ToCursorAsync(ct);

            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var item in cursor.Current)
                {
                    ct.ThrowIfCancellationRequested();

                    yield return item;
                }
            }
        }

        public static async IAsyncEnumerable<TProjection> ToAsyncEnumerable<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var cursor = await find.ToCursorAsync(ct);

            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var item in cursor.Current)
                {
                    ct.ThrowIfCancellationRequested();

                    yield return item;
                }
            }
        }
    }
}
