// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Id should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class UsersTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public UsersTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_create_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                }
            }
        };

        var users = await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);

        Assert.Equal(2, users.Count);

        var user1 = users.ElementAt(0);
        var user2 = users.ElementAt(1);

        Assert.Equal(userId1, user1.Id);
        Assert.Equal(userId2, user2.Id);
        Assert.Equal("name1_0", user1.FullName);
        Assert.Equal("name2_0", user2.FullName);

        await Verify(user1)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_find_user(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = userId1
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = userId2
                }
            }
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Query users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, userId1);

        Assert.Equal(1, users.Total);
        Assert.Equal(userId1, users.Items[0].Id);

        await Verify(users)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_update_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                }
            }
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Update user.
        var userRequest2 = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_1"
                }
            }
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest2);


        // Get users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, take: 100000);

        var user1 = users.Items.SingleOrDefault(x => x.Id == userId1);
        var user2 = users.Items.SingleOrDefault(x => x.Id == userId2);

        Assert.Equal("name1_1", user1?.FullName);
        Assert.Equal("name2_0", user2?.FullName);

        await Verify(user1)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_delete_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                }
            }
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Delete user.
        await _.GetClient(mode).Users.DeleteUserAsync(_.AppId, userId1);


        // Get users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, take: 100000);

        var user1 = users.Items.SingleOrDefault(x => x.Id == userId1);
        var user2 = users.Items.SingleOrDefault(x => x.Id == userId2);

        Assert.Null(user1);
        Assert.NotNull(user2);
    }
}
