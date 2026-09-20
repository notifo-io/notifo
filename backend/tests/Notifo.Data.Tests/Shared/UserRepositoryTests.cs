// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;

namespace Notifo.Shared;

public abstract class UserRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<IUserRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_user()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1") with
        {
            AllowedTopics = ReadonlyList.Create("news"),
            PhoneNumber = "+49123456789",
            Properties = new Dictionary<string, string>
            {
                ["key"] = "value"
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(user);

        var (result, etag) = await sut.GetAsync(appId, user.Id);

        result.Should().BeEquivalentTo(user);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_user_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetAsync(appId, "unknown");

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_return_null_if_user_belongs_to_other_app()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        var (result, _) = await sut.GetAsync(Guid.NewGuid().ToString(), user.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_update_user_with_etag()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        var (_, etag) = await sut.GetAsync(appId, user.Id);

        await sut.UpsertAsync(user with { FullName = "Updated" }, etag);

        var (result, newEtag) = await sut.GetAsync(appId, user.Id);

        Assert.Equal("Updated", result!.FullName);
        Assert.NotEqual(etag, newEtag);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(user, "invalid"));
    }

    [Fact]
    public async Task Should_delete_user()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);
        await sut.DeleteAsync(appId, user.Id);

        var (result, _) = await sut.GetAsync(appId, user.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_delete_user_of_other_app()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);
        await sut.DeleteAsync(Guid.NewGuid().ToString(), user.Id);

        var (result, _) = await sut.GetAsync(appId, user.Id);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_get_user_by_api_key()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        var (result, etag) = await sut.GetByApiKeyAsync(user.ApiKey);

        Assert.Equal(user.Id, result?.Id);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_api_key_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetByApiKeyAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_throw_exception_if_api_key_is_used_by_other_user()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        await Assert.ThrowsAsync<UniqueConstraintException>(() => sut.UpsertAsync(CreateUser("user2") with { ApiKey = user.ApiKey }));
    }

    [Fact]
    public async Task Should_get_user_by_new_api_key_after_update()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        var (_, etag) = await sut.GetAsync(appId, user.Id);

        var newApiKey = Guid.NewGuid().ToString();

        await sut.UpsertAsync(user with { ApiKey = newApiKey }, etag);

        var (result1, _) = await sut.GetByApiKeyAsync(user.ApiKey);
        var (result2, _) = await sut.GetByApiKeyAsync(newApiKey);

        Assert.Null(result1);
        Assert.Equal(user.Id, result2?.Id);
    }

    [Fact]
    public async Task Should_query_user_by_new_email_address_after_update()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1") with { EmailAddress = "john@email.com" };

        await sut.UpsertAsync(user);

        var (_, etag) = await sut.GetAsync(appId, user.Id);

        await sut.UpsertAsync(user with { EmailAddress = "jane@email.com" }, etag);

        var result1 = await sut.QueryAsync(appId, new UserQuery { Query = "john@" });
        var result2 = await sut.QueryAsync(appId, new UserQuery { Query = "jane@" });

        Assert.Empty(result1);
        Assert.Equal([user.Id], result2.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_throw_exception_if_existing_user_is_updated_with_api_key_of_other_user()
    {
        var sut = await CreateSutAsync();

        var user1 = CreateUser("user1");
        var user2 = CreateUser("user2");

        await sut.UpsertAsync(user1);
        await sut.UpsertAsync(user2);

        var (_, etag) = await sut.GetAsync(appId, user2.Id);

        await Assert.ThrowsAsync<UniqueConstraintException>(() => sut.UpsertAsync(user2 with { ApiKey = user1.ApiKey }, etag));
    }

    [Fact]
    public async Task Should_get_user_by_property()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1") with
        {
            Properties = new Dictionary<string, string>
            {
                ["key"] = "value1"
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(user);
        await sut.UpsertAsync(CreateUser("user2") with
        {
            Properties = new Dictionary<string, string>
            {
                ["key"] = "value2"
            }.ToReadonlyDictionary()
        });

        var (result, etag) = await sut.GetByPropertyAsync(appId, "key", "value1");

        Assert.Equal(user.Id, result?.Id);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_get_user_by_multiple_properties()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1") with
        {
            Properties = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
                ["key3"] = "value3"
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(user);

        var (result1, _) = await sut.GetByPropertyAsync(appId, "key1", "value1");
        var (result3, _) = await sut.GetByPropertyAsync(appId, "key3", "value3");

        Assert.Equal(user.Id, result1?.Id);
        Assert.Equal(user.Id, result3?.Id);
    }

    [Fact]
    public async Task Should_not_get_user_by_changed_property()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1") with
        {
            Properties = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }.ToReadonlyDictionary()
        };

        await sut.UpsertAsync(user);

        var (_, etag) = await sut.GetAsync(appId, user.Id);

        await sut.UpsertAsync(
            user with
            {
                Properties = new Dictionary<string, string>
                {
                    ["key1"] = "updated"
                }.ToReadonlyDictionary()
            }, etag);

        var (result1, _) = await sut.GetByPropertyAsync(appId, "key1", "value1");
        var (result2, _) = await sut.GetByPropertyAsync(appId, "key2", "value2");
        var (result3, _) = await sut.GetByPropertyAsync(appId, "key1", "updated");

        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(user.Id, result3?.Id);
    }

    [Fact]
    public async Task Should_return_null_if_property_not_found()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1") with
        {
            Properties = new Dictionary<string, string>
            {
                ["key"] = "value"
            }.ToReadonlyDictionary()
        });

        var (result, etag) = await sut.GetByPropertyAsync(appId, "key", "other");

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_not_get_user_by_property_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1") with
        {
            AppId = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, string>
            {
                ["key"] = "value"
            }.ToReadonlyDictionary()
        });

        var (result, _) = await sut.GetByPropertyAsync(appId, "key", "value");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_query_user_ids()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1"));
        await sut.UpsertAsync(CreateUser("user2"));
        await sut.UpsertAsync(CreateUser("user3") with { AppId = Guid.NewGuid().ToString() });

        var result = new List<string>();

        await foreach (var id in sut.QueryIdsAsync(appId))
        {
            result.Add(id);
        }

        Assert.Equal(["user1", "user2"], result.Order());
    }

    [Fact]
    public async Task Should_query_users_by_id()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("hello"));
        await sut.UpsertAsync(CreateUser("world"));

        var result = await sut.QueryAsync(appId, new UserQuery { Query = "HELLO" });

        Assert.Equal(["hello"], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_users_by_full_name()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1") with { FullName = "John Doe" });
        await sut.UpsertAsync(CreateUser("user2") with { FullName = "Jane Roe" });

        var result = await sut.QueryAsync(appId, new UserQuery { Query = "john" });

        Assert.Equal(["user1"], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_store_and_query_user_with_long_full_name()
    {
        var sut = await CreateSutAsync();

        var fullName = $"John Doe {new string('x', 5000)}";

        await sut.UpsertAsync(CreateUser("user1") with { FullName = fullName });

        var (stored, _) = await sut.GetAsync(appId, "user1");

        var result = await sut.QueryAsync(appId, new UserQuery { Query = "john" });

        Assert.Equal(fullName, stored?.FullName);
        Assert.Equal(["user1"], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_users_by_email_address()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1") with { EmailAddress = "john@email.com" });
        await sut.UpsertAsync(CreateUser("user2") with { EmailAddress = "jane@email.com" });

        var result = await sut.QueryAsync(appId, new UserQuery { Query = "JANE@" });

        Assert.Equal(["user2"], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_users_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1"));
        await sut.UpsertAsync(CreateUser("user2"));
        await sut.UpsertAsync(CreateUser("user3"));

        var result = await sut.QueryAsync(appId, new UserQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_users_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateUser("user1") with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new UserQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_write_counters_to_existing_user()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        await sut.BatchWriteAsync(
        [
            ((appId, user.Id), new CounterMap { ["counter1"] = 1 })
        ], default);

        await sut.BatchWriteAsync(
        [
            ((appId, user.Id), new CounterMap { ["counter1"] = 2, ["counter2"] = 5 })
        ], default);

        var (result, _) = await sut.GetAsync(appId, user.Id);

        Assert.Equal(3, result!.Counters["counter1"]);
        Assert.Equal(5, result!.Counters["counter2"]);
    }

    [Fact]
    public async Task Should_not_create_user_when_writing_counters()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            ((appId, "user1"), new CounterMap { ["counter1"] = 1 })
        ], default);

        var (result, _) = await sut.GetAsync(appId, "user1");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ignore_empty_counters()
    {
        var sut = await CreateSutAsync();

        var user = CreateUser("user1");

        await sut.UpsertAsync(user);

        await sut.BatchWriteAsync(
        [
            ((appId, user.Id), [])
        ], default);

        var (result, _) = await sut.GetAsync(appId, user.Id);

        Assert.Empty(result!.Counters);
    }

    private User CreateUser(string id)
    {
        return new User(appId, id, now)
        {
            // The API key has a unique index over all apps.
            ApiKey = Guid.NewGuid().ToString(),
            EmailAddress = $"{id}@notifo.io",
            FullName = id,
            LastUpdate = now
        };
    }
}
