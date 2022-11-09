// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Identity.MongoDb;

public sealed class MongoDbRoleStore : MongoDbRepository<IdentityRole>, IRoleStore<IdentityRole>
{
    static MongoDbRoleStore()
    {
        BsonClassMap.RegisterClassMap<IdentityRole<string>>(cm =>
        {
            cm.AutoMap();

            cm.MapMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));

            cm.UnmapMember(x => x.ConcurrencyStamp);
        });
    }

    public MongoDbRoleStore(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Identity_Roles";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<IdentityRole> collection,
        CancellationToken ct = default)
    {
        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<IdentityRole>(
                IndexKeys
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions
                {
                    Unique = true
                }),
            cancellationToken: ct);
    }

    protected override MongoCollectionSettings CollectionSettings()
    {
        return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
    }

    public void Dispose()
    {
    }

    public async Task<IdentityRole> FindByIdAsync(string roleId,
        CancellationToken cancellationToken)
    {
        return await Collection.Find(x => x.Id == roleId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        return await Collection.Find(x => x.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        await Collection.InsertOneAsync(role, null, cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        await Collection.ReplaceOneAsync(x => x.Id == role.Id, role, cancellationToken: cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        await Collection.DeleteOneAsync(x => x.Id == role.Id, null, cancellationToken);

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName,
        CancellationToken cancellationToken)
    {
        role.Name = roleName;

        return Task.CompletedTask;
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName,
        CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;

        return Task.CompletedTask;
    }
}
